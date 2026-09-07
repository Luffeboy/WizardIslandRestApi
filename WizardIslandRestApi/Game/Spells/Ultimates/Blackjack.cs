using Swashbuckle.AspNetCore.SwaggerGen;
using WizardIslandRestApi.Game.Spells.BasicSpells.LuckSpells;
using WizardIslandRestApi.Game.Spells.Debuffs;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Blackjack : Spell
    {
        private bool _isPlaying = false;
        private BlackjackGame? _currentGame = null;
        private int _accumulatedWins = 0;

        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;

        public override string Name => (!_isPlaying ? "Blackjack" : 
            (_currentGame == null ? $"release ({_accumulatedWins})" : 
            (_currentGame.IsPlayerTurn ? "Stand / Hit" :
            "Wait for dealer")));

        public int MaxAllowedWins 
        { 
            get => StandardStats.OtherStatsInt[SpellSpecificStats.Luck] * 3;
        }

        public Blackjack(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
            StandardStats.Damage = 3;
            StandardStats.Knockback = 1.1f;

            StandardStats.OtherStatsInt.Add(SpellSpecificStats.Luck, 1);

            Tags.Add(SpellTags.Luck);
        }

        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            // check if we have a bet going
            if (!_isPlaying)
            {
                StartBlackjackGame();
                return;
            }
            // check if the bets have ended, and we release the actual spell
            if (_currentGame == null)
            {
                CreateWinEffect(startPos, mousePos);
                return;
            }
            // check if it is the player's turn
            if (_currentGame.IsPlayerTurn)
            {
                Vector2 direction = mousePos - startPos;
                bool isHit = direction.x > 0; // Right = Hit, Left = Stand

                if (isHit)
                    _currentGame.PlayerHit();
                if (!isHit || !_currentGame.IsPlayerTurn)
                    Stand();
            }
            // it is currently the dealer's turn, so we wait for them to finish their turn
        }

        private void CreateWinEffect(Vector2 startPos, Vector2 mousePos)
        {
            int iterations = _accumulatedWins; // number of projectiles to create

            _isPlaying = false;
            _accumulatedWins = 0;
            GoOnCooldown();

            if (iterations <= 0)
                return;
            
            Dictionary<DeckOfCards, Vector2> deckAndStartPositions = [];
            List<DeckOfCards> decks = new List<DeckOfCards>();
            for (int i = 0; i < iterations; i++)
            {
                var newDeck = new DeckOfCards(MyPlayer);
                deckAndStartPositions.Add(newDeck, startPos);
                newDeck.FullReset();
                decks.Add(newDeck);
                newDeck.StandardStats.Damage = StandardStats.Damage;
                newDeck.StandardStats.Knockback = StandardStats.Knockback;
                newDeck.StandardStats.Speed *= 1.5f;
                newDeck.StandardStats.Range /= 2f;
                newDeck.StandardStats.OtherStatsInt[SpellSpecificStats.Luck] = StandardStats.OtherStatsInt[SpellSpecificStats.Luck];
                newDeck.Observers.WentOnCooldown += (a, b) =>
                {
                    decks.Remove(newDeck);
                };
            }

            int spellToCastNow = -1;
            int castDelay = Math.Max(Game._updatesPerSecond / (iterations), 10);
            var game = GetCurrentGame();

            void CastCards(DeckOfCards deck)
            {
                // is just going to loop forever, if only the player who cast the spell is in the game.
                if (game.Players.Count == 1)
                    return;

                Vector2 castPos = deckAndStartPositions[deck];
                Player? closestPlayer = null;
                float closestDistSqr = 9999999f;
                foreach (var player in game.Players.Values)
                    if (player != MyPlayer && !player.IsDead)
                    {
                        float distSqr = (player.Pos - mousePos).LengthSqr();
                        if (distSqr < closestDistSqr)
                        {
                            closestPlayer = player;
                            closestDistSqr = distSqr;
                        }
                    }
                // no player found
                if (closestPlayer == null)
                {
                    game.ScheduleAction(castDelay, () => CastCards(deck));
                    return;
                }
                
                deck.CastSpell(castPos, closestPlayer.Pos);
                Entity newestEntity = game.Entities.Last();
                if (newestEntity != null)
                    newestEntity.Observers.Expired += (a, b) =>
                    {
                        if (!decks.Contains(deck))
                            return;
                        deckAndStartPositions[deck] = (a as Entity).Pos;
                        mousePos = deckAndStartPositions[deck]; // set target position to the spawn point so it chooses the closest enemy
                        // wait half a second if it hit a player, else just cast it again immediately
                        game.ScheduleAction(b == EntityExpiredReason.CollisionWithPlayer ? Game._updatesPerSecond / 2 : 1, () => CastCards(deck));
                    };
            }
            for (int i = 0; i < iterations; i++)
            {
                var deck = decks[i];
                GetCurrentGame().ScheduleAction(castDelay * (1+i), () => CastCards(deck));
            }
        }

        private void Stand()
        {
            _currentGame.PlayerStand();
            // when standing, go the "dealer" draws cards until they reach 17 or higher, then compare hands
            var game = GetCurrentGame();
            int ticksBetweenDealerActions = Game._updatesPerSecond / (2 + StandardStats.OtherStatsInt[SpellSpecificStats.Luck]);
            void DealerAction()
            {
                if (_currentGame.DealerShouldDraw())
                {
                    _currentGame.DealerHit();
                    game.ScheduleAction(ticksBetweenDealerActions, DealerAction);
                    return;
                }

                HandleGameOutcome(_currentGame.GetOutcome());
            }

            game.ScheduleAction(ticksBetweenDealerActions, DealerAction);
        }

        private void HandleGameOutcome(BlackjackOutcome outcome)
        {
            var game = GetCurrentGame();
            if (outcome == BlackjackOutcome.Tie) // when we get a tie, we just pick a random winner
            {
                Random r = new();
                float winThreshold = 1f / (1 + StandardStats.OtherStatsInt[SpellSpecificStats.Luck]);
                float randomValue = r.NextSingle();
                outcome = randomValue < winThreshold ? BlackjackOutcome.DealerWin : BlackjackOutcome.PlayerWin;
            }
            if (outcome == BlackjackOutcome.PlayerBlackjack)
            {
                _accumulatedWins++;
                outcome = BlackjackOutcome.PlayerWin;
            }

            if (outcome == BlackjackOutcome.PlayerWin)
            {
                if (++_accumulatedWins < MaxAllowedWins)
                {
                    game.Entities.Add(new FollowPlayerEntity(MyPlayer, new Vector2(), Game._updatesPerSecond / 3) { Size = 2, Color = "0,255,0" });
                    StartBlackjackGame();
                }
                // reach max wins, so we get ready to release the spell
                else outcome = BlackjackOutcome.DealerWin;
            }
            if (outcome == BlackjackOutcome.DealerWin)
            {
                game.Entities.Add(new FollowPlayerEntity(MyPlayer, new Vector2(), Game._updatesPerSecond / 3) { Size = 2, Color = "255,0,0" });
                _currentGame = null;
                // since the player has 0 wins, we just reset everything
                if (_accumulatedWins <= 0)
                    CreateWinEffect(new Vector2(), new Vector2());
            }
        }

        private void StartBlackjackGame()
        {
            _isPlaying = true;
            _currentGame = new BlackjackGame(MyPlayer)
            {
                PlayerLuck = StandardStats.OtherStatsInt[SpellSpecificStats.Luck],
            };
            _currentGame.DealInitialCards();
            if (!_currentGame.IsPlayerTurn) // got 21
                Stand();
        }
    }

    public class BlackjackGame
    {
        private Random _random = new Random();

        private Player _player;
        private FollowPlayerEntity _hitHelperEntity;
        private FollowPlayerEntity _standHelperEntity;
        private NamedBuff _playerHandBuff;
        private NamedBuff _dealerHandBuff;

        public int PlayerHand { get; private set; } = 0;
        public int DealerHand { get; private set; } = 0;
        public int PlayerCardCount { get; private set; } = 0;
        public int DealerCardCount { get; private set; } = 0;
        public bool PlayerBusted { get => PlayerHand > 21; }
        public bool DealerBusted { get => DealerHand > 21; }
        public bool IsPlayerTurn { get; private set; } = true;
        public int PlayerLuck { get; set; } = 0;

        public BlackjackGame(Player player)
        {
            _player = player;
            _player.ApplyDebuff(_playerHandBuff = new NamedBuff(_player) { MyName = "BlackjackPlayer" });
            _player.ApplyDebuff(_dealerHandBuff = new NamedBuff(_player) { MyName = "BlackjackDealer" });
            var game = _player.GetGame();
            float size = 3;
            game.Entities.Add(_hitHelperEntity = new FollowPlayerEntity(_player, new Vector2(3 + size, 0), visableTo: _player.Id)
            {
                EntityId = "BlackjackHitHelper",
                Size = size,
            });
            game.Entities.Add(_standHelperEntity = new FollowPlayerEntity(_player, new Vector2(-(3 + size), 0), visableTo: _player.Id)
            {
                EntityId = "BlackjackStandHelper",
                Size = size,
            });
        }

        public void DealInitialCards()
        {
            // Player gets 2 cards
            PlayerHit();
            PlayerHit();

            // Dealer gets 2 cards (only 1 face up in standard blackjack)
            DealerHit();
            DealerHit();

            // Check for natural blackjack
            if (PlayerHand == 21 && PlayerCardCount == 2)
                IsPlayerTurn = false;
        }

        public void PlayerHit()
        {
            if (!IsPlayerTurn || PlayerBusted)
                return;
            int bestValue = DrawCard(PlayerHand);
            for (int i = 0; i < PlayerLuck - 1; i++)
            {
                int newValue = DrawCard(PlayerHand);
                if (bestValue > 21 || (newValue > bestValue && newValue <= 21))
                    bestValue = newValue;
            }
            PlayerHand = bestValue;
            PlayerCardCount++;

            if (PlayerHand >= 21)
                IsPlayerTurn = false;
            _playerHandBuff.Stacks = PlayerHand;
        }

        public void PlayerStand()
        {
            IsPlayerTurn = false;
        }

        public bool DealerShouldDraw()
        {
            return DealerHand < 17 && !PlayerBusted;
        }

        public void DealerHit()
        {
            if (DealerBusted)
                return;
            int worstValue = DrawCard(DealerHand);
            for (int i = 0; i < PlayerLuck - 1; i++)
            {
                int newValue = DrawCard(DealerHand);
                if (newValue < worstValue)
                    worstValue = newValue;
            }
            DealerHand = worstValue;
            DealerCardCount++;
            _dealerHandBuff.Stacks = DealerHand;
        }

        public BlackjackOutcome GetOutcome()
        {
            OnGameEnd();
            if (PlayerHand == 21 && PlayerCardCount == 2)
                return BlackjackOutcome.PlayerBlackjack;

            if (PlayerBusted)
                return BlackjackOutcome.DealerWin;
            if (DealerBusted)
                return BlackjackOutcome.PlayerWin;

            if (PlayerHand > DealerHand)
                return BlackjackOutcome.PlayerWin;
            if (DealerHand > PlayerHand)
                return BlackjackOutcome.DealerWin;

            return BlackjackOutcome.Tie;
        }

        private void OnGameEnd()
        {
            _hitHelperEntity.TicksTillDeletion = -1;
            _standHelperEntity.TicksTillDeletion = -1;
            _playerHandBuff.ShouldRemove = true;
            _dealerHandBuff.ShouldRemove = true;
        }

        private int DrawCard(int hand)
        {
            // Return 1-11 representing card values (1-10 + ace)
            int nextCard = _random.Next(1, 12); // 1-11
            hand += nextCard;
            if (hand > 21 && nextCard == 11)
            {
                // If busting with an Ace, count it as 1 instead of 11
                hand -= 10;
            }
            return hand;
        }
    }

    public enum BlackjackOutcome
    {
        PlayerBlackjack,  // Natural 21 in 2 cards
        PlayerWin,
        DealerWin,
        Tie
    }

    public class NamedBuff : DebuffBase
    {
        public bool ShouldRemove { get; set; } = false;

        public string MyName { get; set; } = "NamedBuff";

        public NamedBuff(Player player) : base(player)
        {
            Stackable = true;
        }

        public override void OnApply()
        {}

        public override void OnRemove()
        {}

        public override bool Update()
        {
            return ShouldRemove;
        }

        public override string ToString()
        {
            return MyName;
        }
    }
}
