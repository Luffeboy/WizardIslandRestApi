using WizardIslandRestApi.Game.Spells.Debuffs;
using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Spells.BasicSpells.BrickSpells
{
    public class BrickGolem : BrickSpell
    {
        public override string Name => "Brick golem";

        public override int CooldownMax { get; protected set; } = 60 * Game._updatesPerSecond;

        public BrickGolem(Player player) : base(player, 5)
        {
            MinBricksToCast = 5;
            StandardStats.Damage = 6;
            StandardStats.Knockback = 2;
            StandardStats.Speed = 0.2f;
            StandardStats.Size = 2.0f;
            StandardStats.SummonLifetime = 30 * Game._updatesPerSecond;
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.SummonQuantity, 1);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.EntityHealth, 10);
            StandardStats.OtherStatsInt.Add(SpellSpecificStats.ActivationDelay, (int)(.1f * Game._updatesPerSecond));

            Tags.Add(SpellTags.Summon);
            Tags.Add(SpellTags.Brick);
        }

        protected override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            // Position a bit in front of player
            Vector2 dir = (mousePos - startPos);
            if (dir.LengthSqr() == 0)
                dir = new Vector2(1, 0);
            dir = dir.Normalized();

            Vector2 spawnPos = startPos + dir * (MyPlayer.Size + StandardStats.Size + 0.5f);
            for (int i = 0; i < StandardStats.OtherStatsInt[SpellSpecificStats.SummonQuantity]; i++)
            {
                GetCurrentGame().Entities.Add(new BrickGolemEntity(MyPlayer, StandardStats.SummonLifetime, spawnPos, StandardStats.Size)
                {
                    Damage = StandardStats.Damage,
                    Knockback = StandardStats.Knockback,
                    Speed = StandardStats.Speed,
                    Health = StandardStats.OtherStatsInt[SpellSpecificStats.EntityHealth],
                    BricksToReturnOnDeath = i == 0 ? MinBricksToCast : 0,
                    AttackActivationTimeTicks = StandardStats.OtherStatsInt[SpellSpecificStats.ActivationDelay],
                });
                spawnPos -= dir * StandardStats.Size;
            }
            // consume bricks and put spell on cooldown
            GoOnCooldownBrick(MinBricksToCast);
        }
    }

    public class BrickGolemEntity : Entity
    {
        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
        private int _attackCooldown = 0;
        private const int _attackCooldownMax = (int)(1 * Game._updatesPerSecond);
        private Player _ownerPlayer;
        private Vector2 _targetPos;
        private Vector2 _handTargetPos;
        private List<Entity> _hitEntities = [];
        private List<ShadowEntity> _hands = [];

        public float Speed { get; set; } = 0.3f;

        public float Damage { get; set; } = 1;

        public float Knockback { get; set; } = 1;

        public int Health { get; set; } = 10;

        public int AttackActivationTimeTicks { get; set; } = 1;

        public int BricksToReturnOnDeath { get; set; } = 1;

        public BrickGolemEntity(Player owner, int ticksUntilDeletion, Vector2 startPos, float size) : base(owner, startPos)
        {
            _ownerPlayer = owner;
            TicksUntilDeletion = ticksUntilDeletion;
            EntityId = "BrickGolem";
            Color = BrickBuff.BrickColor;
            Height = EntityHeight.Normal;
            Size = size;
            // create "hands"
            for (int i = 0; i < 2; i++)
            {
                _hands.Add(new ShadowEntity()
                {
                    Color = Color,
                    Size = Size / 2,
                    Pos = Pos,
                    TicksUntilDeletion = 99999,
                    EntityId = "BrickGolemHand",
                });
                _ownerPlayer._game.Entities.Add(_hands[i]);
            }
        }

        public int TicksUntilDeletion
        {
            get => _ticksUntilDeletion;
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = value;
            }
        }

        public override void ReTarget(Vector2 pos)
        {
            _ticksUntilDeletion = _ticksUntilDeletionMax;
            _targetPos = pos;
        }

        public override bool Update()
        {
            Vector2 handForward;
            Vector2 handSideDir;
            float distanceToSide = Size * 2 / 3;
            if (--_attackCooldown <= 0)
            {
                // find nearest enemy player in aggro radius
                Player? nearest = null;
                float nearestDistSqr = float.MaxValue;
                foreach (var p in MyCollider.Owner.GetGame().Players.Values)
                {
                    if (p == MyCollider.Owner || p.IsDead)
                        continue;
                    float distSqr = (p.Pos - Pos).LengthSqr();
                    if (distSqr < nearestDistSqr)
                    {
                        nearest = p;
                        nearestDistSqr = distSqr;
                    }
                }

                if (nearest != null)
                    _targetPos = nearest.Pos;
                else
                {
                    var toOwner = MyCollider.Owner.Pos - Pos + new Vector2(0, -Size);
                    if (toOwner.LengthSqr() > 1f)
                        _targetPos = Pos + toOwner;
                    else
                        _targetPos = Pos;
                }
                // Move toward target pos
                Vector2 moveDir = (_targetPos - Pos).Normalized();
                Pos += moveDir * Speed;
                handForward = moveDir;
                handSideDir = handForward.Normal();
            }
            else // has just attacked
            {
                handForward = (_handTargetPos - Pos);
                handSideDir = handForward.Normal().Normalized();
                distanceToSide *= (float)(_attackCooldown - _attackCooldownMax / 2) / (float)_attackCooldownMax * 2;
                float minDist = .25f;
                distanceToSide = distanceToSide < 0 ? MathF.Min(distanceToSide, -minDist) : MathF.Max(distanceToSide, minDist);
            }

            // move hands
            if (handForward.x != 0 || handForward.y != 0)
            {
                _hands[0].Pos = Pos + handForward + handSideDir * distanceToSide;
                _hands[1].Pos = Pos + handForward - handSideDir * distanceToSide;
                ForwardAngle = MathF.Atan2(handForward.y, handForward.x);
                _hands[0].ForwardAngle = ForwardAngle;
                _hands[1].ForwardAngle = ForwardAngle;
            }

            return --_ticksUntilDeletion < 0;
        }

        public override bool OnCollision(Entity other)
        {
            if (other is BrickGolemEntity golem)
            {
                Vector2 dir = (Pos - other.Pos).Normalized();
                if (dir.LengthSqr() == 0) 
                    dir.x = new Random().NextSingle() - .5f;
                float moveAmount = .1f;
                Pos += dir * moveAmount;
                return false;
            }
            if (!base.OnCollision(other) || _hitEntities.Contains(other))
                return false;
            _hitEntities.Add(other);
            return --Health < 0;
        }

        public override bool OnCollision(Player other)
        {
            // ignore owner
            if (other == MyCollider.Owner)
                return false;

            // attack
            if (_attackCooldown <= 0)
            {
                Vector2 attackPoint = Pos + (other.Pos - Pos).Normalized() * Size;
                var game = MyCollider.Owner._game;
                game.Entities.Add(new MeteorEntity(MyCollider.Owner, attackPoint, game)
                {
                    Size = Size * 2 / 3,
                    FallTime = AttackActivationTimeTicks,
                    Damage = Damage,
                    KnockbackMin = Knockback * 0.8f,
                    KnockbackMax = Knockback * 1.2f,
                });
                _handTargetPos = attackPoint;
                _attackCooldown = _attackCooldownMax;
            }

            return false;
        }

        public override void OnExpire(EntityExpiredReason reason)
        {
            base.OnExpire(reason);
            var game = MyCollider.Owner._game;
            Random r = new();
            for (int i = 0; i < BricksToReturnOnDeath; i++)
                game.Entities.Add(new BrickEntity(_ownerPlayer, _ticksUntilDeletionMax, Pos, .5f)
                {
                    Damage = 0,
                    Knockback = 0,
                    Dir = new Vector2(MathF.Cos(r.NextSingle() * 2f * MathF.PI), MathF.Sin(r.NextSingle() * 2f * MathF.PI)),
                });
            
            foreach (var hand in _hands)
                hand.TicksUntilDeletion = -1;
        }
    }
}
