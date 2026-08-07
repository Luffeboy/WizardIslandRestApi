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

        public override string Name => (!_isPlaying ? "Blackjack" : (_currentGame == null ? $"release ({_accumulatedWins})" : "Stand / Hit"));

        public int MaxAllowedWins 
        { 
            get => StandardStats.OtherStatsInt[SpellSpecificStats.Luck] * 3;
        }

        public Blackjack(Player player) : base(player)
        {
            Type = SpellType.Ultimate;

            StandardStats.OtherStatsInt.Add(SpellSpecificStats.Luck, 1);
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
            Console.WriteLine("Wins: " + _accumulatedWins);

            _isPlaying = false;
            _accumulatedWins = 0;
            GoOnCooldown();
        }

        private void Stand()
        {
            _currentGame.PlayerStand();
            // when standing, go the "dealer" draws cards until they reach 17 or higher, then compare hands
            var game = GetCurrentGame();
            int ticksBetweenDealerActions = Game._updatesPerSecond / 3;
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
