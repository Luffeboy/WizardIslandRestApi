using WizardIslandRestApi.Game.Spells.ExtraEntities;
namespace WizardIslandRestApi.Game.Spells.BasicSpells.LuckSpells
{
    public class DeckOfCards : Spell
    {
        float Damage { get; set; } = 5.0f;
        float knockback { get; set; } = 1.5f;
        private int _numberOfCards = 10;
        private int _nextCardNumber = 0;
        private CardEntity _nextCard = null;
        public int CooldownBetweenCardsInSameDeck { get; protected set; } = (int)(.5f * Game._updatesPerSecond);
        public override int CooldownMax { get; protected set; } = (int)(5.0f * Game._updatesPerSecond);
        public override string Name => _nextCard == null ? "Deck Of Cards" : $"Card: {_nextCard.Number}";

        public DeckOfCards(Player player) : base(player)
        {
            if (player is null)
                return;
            GetNewCard();
        }

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if (!_nextCard.HasBeenCast)
            {
                _nextCard.HasBeenCast = true;
                _nextCard.TeleportTo(startPos);
                _nextCard.Dir = (mousePos - startPos).Normalized();
                GetCurrentGame().Entities.Add(_nextCard);
                if (_nextCardNumber != 0)
                {
                    GoOnCooldownSameDeck();
                    GetNewCard();
                }
                return;
            }
            _nextCard._onRecast?.Invoke(startPos, mousePos);
        }

        private void GetNewCard()
        {
            _nextCardNumber = (_nextCardNumber + 1) % _numberOfCards;
            string cardNum = _nextCardNumber.ToString();
            if (_nextCardNumber == 0)
                switch (new Random().Next(5))
                {
                    case 0:
                        cardNum = "Ace Hearts";
                        break;
                    case 1:
                        cardNum = "Ace Diamonds";
                        break;
                    case 2:
                        cardNum = "Ace Spades";
                        break;
                    case 3:
                        cardNum = "Ace Clubs";
                        break;
                    default:
                        cardNum = "Joker";
                        break;
                }
            float cardSpeed = 50.0f / Game._updatesPerSecond;
            float cardSize = .25f;
            int lifetime = (int)(2.0f * Game._updatesPerSecond);
            _nextCard = new CardEntity(MyPlayer, lifetime, new Vector2(), cardNum, Damage, knockback, cardSpeed, null,
                _nextCardNumber == 0 ? () =>
            {
                GoOnCooldown();
                GetNewCard();
            } : null)
            {
                Size = cardSize,
            };
        }
        private void GoOnCooldownSameDeck()
        {
#if !NoCooldown
            CurrentCooldown = GetCurrentGameTick() + (int)(CooldownBetweenCardsInSameDeck * GetCurrentGame().GameModifiers.CooldownMultiplier * MyPlayer.Stats.CooldownMultiplier);
#endif
        }
    }

    public class CardEntity : CantHitOwnTypeEntity
    {
        private Action? _goOnCooldown;
        private Action _onDestroy = ()=> { };
        public Action<Vector2, Vector2>? _onRecast { get; private set; } = null;
        private bool _shouldUseOnDestroyWhenHittingPlayer = true;
        public string Number { get; set; }
        public bool HasBeenCast { get; set; } = false;
        public Vector2 Dir { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public float Speed { get; set; }
        public override int TicksUntillCanHitOwner { get; set; } = (int)(.5f * Game._updatesPerSecond);
        public CardEntity(Player owner, int ticksUntilDeletion, Vector2 startPos, string cardNum, float damage, float knockback, float speed, Vector2? dir = null, Action? goOnCooldown = null) : base(owner, ticksUntilDeletion, startPos)
        {
            _goOnCooldown = goOnCooldown;
            Color = "220,220,220";
            Number = cardNum;
            EntityId = $"Card_{cardNum.Replace(' ', '_')}";
            if (dir is not null)
                Dir = dir.Value;
            Damage = damage;
            Knockback = knockback;
            Speed = speed;
            ForwardAngle = MathF.Atan2(Dir.y, Dir.x);
            {
                var game = owner.GetGame();
                switch (Number)
                {
                    case "Ace Hearts":
                        int fullLifeTime = _ticksUntilDeletion;
                        _onDestroy = () =>
                        {
                            int healAmount = Math.Max(20 * (fullLifeTime / _ticksUntilDeletionMax), 5);
                            owner.Heal(healAmount);
                        };
                        _onRecast = (x,y) => { fullLifeTime = _ticksUntilDeletion; _ticksUntilDeletion = 0; };
                        break;
                    case "Ace Diamonds":
                        _shouldUseOnDestroyWhenHittingPlayer = false;
                        _onDestroy = () =>
                        {
                            int amountOfCards = 8;
                            float angleBetweenCards = MathF.PI / (amountOfCards+0) * 2;
                            int lifetime = _ticksUntilDeletionMax / 2;
                            Random r = new Random();
                            Speed *= 1.5f;
                            Damage /= 2;
                            Knockback /= 2;
                            for (int i = 0; i < amountOfCards; i++)
                            {
                                float angle = ForwardAngle + i * angleBetweenCards;

                                game.Entities.Add(new CardEntity(owner, lifetime, Pos, r.Next(10).ToString(), Damage * 3, Knockback * 3, Speed / 2, new Vector2(MathF.Cos(angle), MathF.Sin(angle)))
                                {
                                    Size = Size,
                                });
                            }
                        };
                        _onRecast = (x, y) => { _ticksUntilDeletion = 0; };
                        break;
                    case "Ace Spades":
                        _onDestroy = () =>
                        {
                            game.Entities.Add(new MeteorEntity(owner, Pos, game)
                            {
                                Size = Size * 25f,
                                Damage = Damage,
                                KnockbackMin = Knockback / 2,
                                KnockbackMax = Knockback,
                                FallTime = 1,
                            });
                        };
                        _onRecast = (x, y) => { _ticksUntilDeletion = 0; };
                        break;
                    case "Ace Clubs":
                        {
                            Speed /= 2;
                            var targetPos = Pos + Dir * 1000;
                            _onDestroy = () =>
                            {
                                game.Entities.Add(new HomingBoltEntity(owner, Pos, targetPos, game)
                                {
                                    Speed = Speed * 3,
                                    Color = "255, 255, 255",
                                    Size = Size,
                                    TicksUntilDeletion = _ticksUntilDeletionMax,
                                    Damage = Damage,
                                    Knockback = Knockback,
                                });
                            };
                            _onRecast = (start, target) => { targetPos = target; };
                        }
                        break;
                    case "Joker":
                        {
                            Vector2? targetPos = null;
                            _onDestroy = () =>
                            {
                                if (targetPos is null)
                                    targetPos = Pos + Dir * 10;
                                var newCardsDir = (targetPos.Value - Pos).Normalized();
                                var diamondCard = new CardEntity(owner, _ticksUntilDeletionMax, Pos, "Ace Diamonds", Damage / 3, Knockback / 3, Speed, newCardsDir) { Size = Size };
                                diamondCard._ticksUntilDeletion = (int)(.33f * Game._updatesPerSecond); // since the ace of diamonds kinda need to explode before hitting anything directly...
                                game.Entities.Add(new CardEntity(owner, _ticksUntilDeletionMax, Pos, "Ace Hearts", Damage / 2, Knockback / 2, Speed, newCardsDir) { Size = Size });
                                game.Entities.Add(diamondCard);
                                game.Entities.Add(new CardEntity(owner, _ticksUntilDeletionMax, Pos, "Ace Spades", Damage / 3, Knockback / 3, Speed, newCardsDir) { Size = Size });
                                game.Entities.Add(new CardEntity(owner, _ticksUntilDeletionMax, Pos, "Ace Clubs", Damage / 3, Knockback / 3, Speed, newCardsDir) { Size = Size });
                            };
                            _onRecast = (start, target) => { targetPos = target; _ticksUntilDeletion = 0; };
                        }
                        break;
                }
            }
        }

        public override void ReTarget(Vector2 pos)
        {
            _ticksUntilDeletion = _ticksUntilDeletionMax;
        }

        public override bool Update()
        {
            Pos += Dir * Speed;
            if (base.Update())
            {
                _onDestroy();
                _goOnCooldown?.Invoke();
                return true;
            }
            return false;
        }

        protected override bool HitPlayer(Player other)
        {
            if (_shouldUseOnDestroyWhenHittingPlayer)
                _onDestroy();
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback(Dir, Knockback);
            _goOnCooldown?.Invoke();
            return true;
        }
    }
}
