using System.Numerics;
using WizardIslandRestApi.Game.Augments.GenericAugments;
using WizardIslandRestApi.Game.Spells;
using static WizardIslandRestApi.Controllers.WizardIslandController;

namespace WizardIslandRestApi.Game.Augments
{
    public class AugmentSystem
    {
        private Game _game;
        public int MaxAugmentsPerPlayer { get; set; } = 5;
        public int AugmentsToChooseFromCount { get; set; } = 3;
        private int _ticksBetweenAugments = -1;
        public int AugmentsGivenSoFar { get; set; } = 0;

        public List<PlayerAugmentConnector> PlayersAndAugmentsTheyCanChoose { get; } = [];
        public Dictionary<Player, PlayerAugmentData> AllPlayerAugmentData { get; } = [];

        private int _playersAndAugmentsTheyCanChooseStartCount = -1;
        private float _timeUntilAugmentPhaseEnds = -1f;
        private const float _timeUntilAugmentPhaseEndsMax = 30;

        public static List<AugmentBase> AllAugments { get; } = [];

        public AugmentSystem(Game game)
        {
            _game = game;
            foreach (var player in _game.Players.Values)
                AllPlayerAugmentData.Add(player, new PlayerAugmentData(player));
            _ticksBetweenAugments = Game._gameDuration / (MaxAugmentsPerPlayer + 1);
            ScheduleAugmentPhase();
        }

        private void ScheduleAugmentPhase()
        {
#if DEBUG
            if (_game.GetCopyOfScheduledActions().Any(actionAndGameTick => actionAndGameTick.MyAction == StartAugmentPhase))
                return;
#endif
            if (AugmentsGivenSoFar == 0)
                _game.ScheduleAction(0, StartAugmentPhase);
            else _game.ScheduleAction(_ticksBetweenAugments, StartAugmentPhase);
        }

        public static void LoadAugments()
        {
            // use reflection to find all classes that inherit from AugmentBase and add them to the list of available augments
            // also exclude any classes that have parameters in their constructors, as they cannot be instantiated without parameters
            var augmentTypes = typeof(AugmentBase).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(AugmentBase)) && !t.IsAbstract && t.GetConstructors().Any(c => c.GetParameters().Length == 0));
#if DEBUG && SHOW_AUGMENTS
            Console.WriteLine("All available augments:");
#endif
            foreach (var augmentType in augmentTypes)
            {
                var augment = (AugmentBase)Activator.CreateInstance(augmentType);
                if (augment != null)
                    AllAugments.Add(augment);
#if DEBUG && SHOW_AUGMENTS
                Console.WriteLine($"Reflection: Augment added: {augment.AugmentName}");
#endif
            }
            // manual augments
            {
                // basic stats
                var standardSpellStats = new StandardSpellStats();
                AllAugments.Add(new GenericSpellStatMultiplier(nameof(standardSpellStats.Speed), 1.25f));
                AllAugments.Add(new GenericSpellStatMultiplier(nameof(standardSpellStats.Damage), 1.25f));
                AllAugments.Add(new GenericSpellStatMultiplier(nameof(standardSpellStats.Knockback), 1.15f));
                AllAugments.Add(new GenericSpellStatMultiplier(nameof(standardSpellStats.Range), 1.3f));
                AllAugments.Add(new GenericSpellStatMultiplier(nameof(standardSpellStats.CooldownMultiplier), .9f));
                AllAugments.Add(new GenericSpellStatMultiplier(nameof(standardSpellStats.Size), 1.25f));

                // more specific stats
                AllAugments.Add(new GenericSpecificSpellStatIncreaseAugment(SpellSpecificStats.ProjectileEmitterCount, 1, "Fire away... more", "emitters"));
                AllAugments.Add(new GenericSpecificSpellStatIncreaseAugment(SpellSpecificStats.SummonQuantity, 1, "More Summons", "all summons"));
                AllAugments.Add(new GenericSpecificSpellStatIncreaseAugment(SpellSpecificStats.SpellUsesMax, 1, "More max uses", "spells that have more uses"));
                
            }
#if DEBUG && SHOW_AUGMENTS
            Console.WriteLine("\nAll augments");
            foreach (var aug in AllAugments)
                Console.WriteLine(aug.AugmentName);

            Console.WriteLine("\nAugments end\n");
#endif
        }

        public List<AugmentBase> GetAugmentsForPlayer(Player player)
        {
#if DEBUG
            return new List<AugmentBase>(AllPlayerAugmentData[player].UsableAugments);
#endif
            Random r = new Random();
            var allAvailableAugmentsToPlayer = new List<AugmentBase>(AllPlayerAugmentData[player].UsableAugments);
            var augmentsToChooseFrom = new List<AugmentBase>();
            for (int i = 0; i < AugmentsToChooseFromCount && allAvailableAugmentsToPlayer.Any(); i++)
            {
                var augIndex = r.Next(allAvailableAugmentsToPlayer.Count);
                AugmentBase randomAugment = allAvailableAugmentsToPlayer[augIndex];
                allAvailableAugmentsToPlayer.RemoveAt(augIndex);
                augmentsToChooseFrom.Add(randomAugment);
            }
            return augmentsToChooseFrom;
        }

        public void AugmentUpdate()
        {
            // check if all players have chosen an augment, or time has run out
            if (PlayersAndAugmentsTheyCanChoose.Count != _playersAndAugmentsTheyCanChooseStartCount)
            {
                _timeUntilAugmentPhaseEnds -= Game.DeltaTime;
                // wait a max of 3 seconds when everyone has chosen an augment
                if (!PlayersAndAugmentsTheyCanChoose.Any())
                {
                    _timeUntilAugmentPhaseEnds = MathF.Min(_timeUntilAugmentPhaseEnds, 3f);
#if DEBUG
                    _timeUntilAugmentPhaseEnds = -1;
#endif
                }
                // resume the game
                if (_timeUntilAugmentPhaseEnds <= 0)
                {
                    EndAugmentPhase();
                    return;
                }
            }
            // send update to players
            SendAugmentDataToPlayers();
        }

        // start augment selection process
        public void StartAugmentPhase()
        {
            AugmentsGivenSoFar++;
            _timeUntilAugmentPhaseEnds = _timeUntilAugmentPhaseEndsMax;
            PlayersAndAugmentsTheyCanChoose.Clear();

            var players = _game.Players;
            foreach (var player in players.Values)
            {
                var augments = GetAugmentsForPlayer(player);
                if (augments.Any())
                    PlayersAndAugmentsTheyCanChoose.Add(new PlayerAugmentConnector(player, augments));
#if DEBUG
                else Console.WriteLine($"Player: {player.Name} was unable to get any augments");
#endif
            }
            _playersAndAugmentsTheyCanChooseStartCount = PlayersAndAugmentsTheyCanChoose.Count;
            if (PlayersAndAugmentsTheyCanChoose.Any())
            {
                _game.SetGameUpdateFunction(AugmentUpdate);
                SendAugmentDataToPlayers();
            }
#if DEBUG
            else Console.WriteLine("No player was able to get any augments");
#endif
        }

        // end augment selection process
        public void EndAugmentPhase()
        {
            _game.SetGameUpdateFunctionToDefault();
            ScheduleAugmentPhase();
        }

        public void PlayerSelectAugment(Player player, int augmentIndex)
        {
            if (augmentIndex < 0)
                return;
            for (int i = 0; i < PlayersAndAugmentsTheyCanChoose.Count; i++)
            {
                var playerAndAugments = PlayersAndAugmentsTheyCanChoose[i];
                if (playerAndAugments.Player != player)
                    continue;
                if (augmentIndex >= playerAndAugments.AugmentsToChooseFromAndSpellsEffected.Count)
                    return;

                var augment = playerAndAugments.AugmentsToChooseFromAndSpellsEffected[augmentIndex].Item1;
                ApplyAugment(augment, player, player.GetOriginalSpells());
                AllPlayerAugmentData[player].AugmentHistory.Add(augment);

                PlayersAndAugmentsTheyCanChoose.RemoveAt(i);
                // maybe also remove the augment from _playersAndAugmentsTheyCanUse, if we don't want repeat-augments
                if (!augment.CanBeStacked)
                {
#if DEBUG
                    if (AllPlayerAugmentData[player].UsableAugments.Remove(augment))
                        Console.WriteLine("Removed augment from augment pool.\n" + augment.AugmentName);
                    else Console.WriteLine("Faield to remove augment from augment pool, even though it shouldnt be able to stack.\n" + augment.AugmentName);
#else
                    AllPlayerAugmentData[player].UsableAugments.Remove(augment);
#endif
                }
                break;
            }
        }

        public void ApplyAugment(AugmentBase augment, Player player, Spell[] spells)
        {
            int spellsThatApplied = 0;
            foreach (var spell in spells)
                if (augment.AttemptAugmentSpell(spell))
                    spellsThatApplied++;
            augment.AugmentPlayer(player, spellsThatApplied);
        }

        public void ReApplyAllAugmentToPlayersSpellsOnly(Player player, Spell[] spells)
        {
            foreach (AugmentBase augment in AllPlayerAugmentData[player].AugmentHistory)
                foreach (var spell in spells)
                    augment.AttemptAugmentSpell(spell);
        }

        public void SendAugmentDataToPlayers()
        {
            foreach (var player in _game.Players.Values)
            {
                var augmentData = PlayersAndAugmentsTheyCanChoose.FirstOrDefault(x => x.Player == player)?
                    .AugmentsToChooseFromAndSpellsEffected.Select(aug =>
                new {
                    AugmentName = aug.Item1.AugmentName,
                    AugmentDescription = aug.Item1.AugmentDescription,
                    AugmentsEffected = aug.Item2,
                });
                player.SendData(PacketToClientType.GetAugment, new
                {
                    TimeRemaining = _timeUntilAugmentPhaseEnds,
                    PlayersWhoHaveNotPickedAugmentsYetCount = PlayersAndAugmentsTheyCanChoose.Count,
                    AugmentData = augmentData,
                });
            }
        }
    }

    public class PlayerAugmentConnector
    {
        public Player Player { get; private set; }
        public List<Tuple<AugmentBase, List<string>>> AugmentsToChooseFromAndSpellsEffected { get; private set; } = [];

        public PlayerAugmentConnector(Player player, List<AugmentBase> augments)
        {
            Player = player;
            var playerSpells = player.GetOriginalSpells();
            foreach (var augment in augments)
            {
                List<string> spellsThatAreEffected = [];
                foreach (var spell in playerSpells)
                    if (augment.CanAugmentSpell(spell))
                        spellsThatAreEffected.Add(spell.ToString());
                AugmentsToChooseFromAndSpellsEffected.Add(new (augment, spellsThatAreEffected));
            }
        }
    }
}
