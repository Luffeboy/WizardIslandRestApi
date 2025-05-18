using System.Drawing;

namespace WizardIslandRestApi.Game.Spells.ExtraEntities
{
    public class PenguinEntity : EntityPlus
    {
        private Vector2 _walkAroundPoint;
        private float _wanderDist = 10;
        private Vector2 _targetPos;
        private int _health;
        private int _healthMax = 5;
        private Random _r = new Random();
        private Action<Player, PenguinEntity> _onPlayerKilledThis;
        private Game _game;
        public Vector2 Vel { get; set; }
        public Player? TargetPlayer { get; set; } = null;
        public float ExplosionSize { get; set; } = 3.0f;
        public PenguinEntity(Player owner, int ticksUntilDeletion, Vector2 startPos, Action<Player, PenguinEntity> onPlayerKilledThis, Game game) : base(owner, ticksUntilDeletion, startPos)
        {
            _walkAroundPoint = startPos;
            _health = _healthMax;
            EntityId = "Penguin";
            _onPlayerKilledThis = onPlayerKilledThis;
            _game = game;
            Color = "255, 255, 255";
            FindTargetAroundWalkAroundPoint();
        }


        public override void ReTarget(Vector2 pos)
        {
            Dir = (pos - Pos).Normalized();
            _targetPos = Pos + Dir * Speed * 3;
            _health = _healthMax;
        }

        public override bool Update()
        {
            Vel *= .95f;
            if (TargetPlayer != null)
            {
                _targetPos = TargetPlayer.Pos;
                Dir = (_targetPos - Pos).Normalized();
                if (TargetPlayer.IsDead)
                {
                    TargetPlayer = null;
                    FindTargetAroundWalkAroundPoint();
                }
            }
            else if (Dir.Dot(_targetPos - Pos) < 0)
                FindTargetAroundWalkAroundPoint();
            Vel += Dir * Speed;
            Pos += Vel;
            ForwardAngle = HomingBoltEntity.GetAngleFromDirection(Vel);
            return --_ticksUntilDeletion < 0;
        }

        protected override bool HitPlayer(Player other)
        {
            Explode();
            return true;
        }
        public override bool OnCollision(Entity other)
        {
            if (other.EntityId == EntityId || other.EntityId == "PenguinExplosion")
            {
                // move a little away from them
                Vel += (Pos - other.Pos).Normalized() * Speed * 3;
                return false; 
            }
            if (base.OnCollision(other))
            {
                if (other.MyCollider.Owner != null)
                    _onPlayerKilledThis(other.MyCollider.Owner, this);
                Explode();
                return true;
            }
            return false;
        }
        private void FindTargetAroundWalkAroundPoint()
        {
            float angle = _r.NextSingle() * MathF.PI * 2;
            float dist = _r.NextSingle() * _wanderDist;
            _targetPos = _walkAroundPoint + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist;
            Dir = (_targetPos - Pos).Normalized();
            Vel = new Vector2();
        }

        private void Explode()
        {
            _game.Entities.Add(new MeteorEntity(MyCollider.Owner, Pos, _game)
            {
                Color = "50, 50, 50",
                Size = ExplosionSize,
                FallTime = 3,
                Damage = Damage,
                KnockbackMin = Knockback * .75f,
                KnockbackMax = Knockback,
                EntityId = "PenguinExplosion",
            });
        }
    }
}
