using System.Xml.Linq;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class SpeedBlits : MultiUseSpell
    {
        private float _maxRange = 10;
        public override string Name => "Speed blitz";
        public SpeedBlits(Player player) : base(player)
        {
            UsesMax = 5;
            CooldownBetweenUses = 3;
            Type = SpellType.Ultimate;
        }

        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;

        public override void OnUse(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 endPos = (mousePos - startPos);
            float len = endPos.Length();
            if (len > _maxRange)
            {
                endPos = endPos / len * _maxRange;
                len = _maxRange;
            }
            endPos = endPos + startPos;

            float speed = 2;
            float size = .5f;
            float damage = 10;
            float knockback = 1.2f;
            string color = "255,255,0";
            Vector2 pos = startPos;
            Vector2 normal = (endPos - startPos).Normal() / len;
            GetCurrentGame().Entities.Add(new SpeedBlitsEntity(MyPlayer, pos + normal * len, pos, GetCurrentGame())
            {
                Speed = speed,
                Damage = damage,
                Knockback = knockback,
                Size = size,
                Color = color,
                Dir = normal * -1,
                TicksUntilDeletion = 100,
            });
            GetCurrentGame().Entities.Add(new SpeedBlitsEntity(MyPlayer, pos + normal * -len, pos, GetCurrentGame())
            {
                Speed = speed,
                Damage = damage,
                Knockback = knockback,
                Size = size,
                Color = color,
                Dir = normal * 1,
                TicksUntilDeletion = 100,
            });

            MyPlayer.TeleportTo(mousePos);
        }
    }

    public class SpeedBlitsEntity : Entity
    {
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public Vector2 Dir { get; set; }

        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;

        private bool _goingBackToPlayer = false;
        private bool _hasSpawnedMeteor = false;
        private Vector2 _meetingPos;
        private Game _game;
        private List<Player> _hitPlayers = new List<Player> ();

        public int TicksUntilDeletion
        {
            get { return _ticksUntilDeletion; }
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
            }
        }
        public SpeedBlitsEntity(Player owner, Vector2 startPos, Vector2 meetingPos, Game game) : base(owner, startPos)
        {
            _meetingPos = meetingPos;
            _game = game;
        }

        public override bool OnCollision(Player other)
        {
            if (other == MyCollider.Owner || _hitPlayers.Contains(other))
                return false;
            _hitPlayers.Add(other);
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback(Dir, Knockback);
            return false;
        }
        public override bool OnCollision(Entity other)
        {
            if (other is SpeedBlitsEntity && !_hasSpawnedMeteor)
            {
                _hasSpawnedMeteor = true;
                _game.Entities.Add(new MeteorEntity(MyCollider.Owner, Pos)
                {
                    FallTime = 10,
                    Damage = Damage,
                    KnockbackMin = Knockback,
                    KnockbackMax = Knockback,
                    Size = 10,
                    Color = "100,100,0",
                    EntityId = "ElectricExplosion",
                });
            }
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            Speed = -Speed;
        }

        public override bool Update()
        {
            Pos += Dir * Speed;
            if (_goingBackToPlayer)
            {
                if ((MyCollider.Owner.Pos - Pos).Dot(Dir) < 0)
                    return true;
            }
            else
            {
                if ((_meetingPos - Pos).Dot(Dir) < 0)
                {
                    Dir = (MyCollider.Owner.Pos - Pos).Normalized();
                    _goingBackToPlayer = true;
                }
            }
            return --_ticksUntilDeletion < 0;
        }
    }
}
