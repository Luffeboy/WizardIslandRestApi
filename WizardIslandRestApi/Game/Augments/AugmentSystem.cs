using System.Net.WebSockets;
using System.Numerics;
using static WizardIslandRestApi.Controllers.WizardIslandController;

namespace WizardIslandRestApi.Game.Augments
{
    public class AugmentSystem
    {
        private Game _game;
        public int AugmentsToChooseFromCount { get; set; } = 3;
        public int MaxAugmentsPerPlayer { get; set; } = 3;
        private int _ticksBetweenAugments = -1;
        //public int AugmentsGivenSoFar { get; set; } = 0;

        public List<PlayerAugmentConnector> PlayersAndAugmentsTheyCanChoose { get; } = [];

        private int _playersAndAugmentsTheyCanChooseStartCount = -1;
        private float _timeUntilAugmentPhaseEnds = -1f;
        private const float _timeUntilAugmentPhaseEndsMax = 30;

        public AugmentSystem(Game game)
        {
            _game = game;
            _ticksBetweenAugments = Game._gameDuration / (MaxAugmentsPerPlayer + 1);
            ScheduleAugmentPhase();
        }

        private void ScheduleAugmentPhase()
        {
            _game.ScheduleAction(_ticksBetweenAugments, StartAugmentPhase);
        }

        public List<AugmentBase> GetAugmentsForPlayer(Player player)
        {
            return [new TestAugment()];
        }

        public void AugmentUpdate()
        {
            // check if a player has selected an augment

            SendAugmentDataToPlayers();
            // check if all players have chosen an augment, or time has run out
            if (PlayersAndAugmentsTheyCanChoose.Count != _playersAndAugmentsTheyCanChooseStartCount &&
                    ((_timeUntilAugmentPhaseEnds -= Game.DeltaTime) < 0
                        || !PlayersAndAugmentsTheyCanChoose.Any()))
            {
                EndAugmentPhase();
            }

            //EndAugmentPhase();
        }

        // start augment selection process
        public void StartAugmentPhase()
        {
#if DEBUG
            Console.WriteLine($"Augmenting... ");
#endif
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
                if (playerAndAugments.Player == player)
                {
                    if (augmentIndex >= playerAndAugments.AugmentsToChooseFrom.Count)
                        return;
                    var augment = playerAndAugments.AugmentsToChooseFrom[augmentIndex];
                    foreach (var spell in player.GetOriginalSpells())
                        augment.AttemptAugmentSpell(spell);

                    PlayersAndAugmentsTheyCanChoose.RemoveAt(i);
                    break;
                }
            }
        }

        public void SendAugmentDataToPlayers()
        {
            foreach (var player in _game.Players.Values)
            {
                var augmentData = PlayersAndAugmentsTheyCanChoose.FirstOrDefault()?.AugmentsToChooseFrom.Select(aug =>
                new {
                    AugmentName = aug.AugmentName,
                    AugmentDescription = aug.AugmentDescription,
                });
                player.SendData(PacketToClientType.GetAugment, new
                {
                    TimeRemaining = _timeUntilAugmentPhaseEndsMax,
                    PlayersWhoHaveNotPickedAugmentsYetCount = PlayersAndAugmentsTheyCanChoose.Count,
                    AugmentData = augmentData,
                });
            }
        }
    }

    public class PlayerAugmentConnector
    {
        public Player Player { get; private set; }
        public List<AugmentBase> AugmentsToChooseFrom { get; private set; }

        public PlayerAugmentConnector(Player player, List<AugmentBase> augments)
        {
            Player = player;
            AugmentsToChooseFrom = augments;
        }
    }
}
