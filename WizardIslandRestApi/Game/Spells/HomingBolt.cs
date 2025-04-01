using System;
namespace WizardIslandRestApi.Game.Spells
{
    public class HomingBolt : Spell
    {
        public override string Name { get { return "Homing bolt"; } }
        private float _damage = 5;
        private float _knockback = 1.5f;
        public override int CooldownMax { get; protected set; } = (int)(5.0f * Game._updatesPerSecond);
        public HomingBolt(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            float size = .5f;
            GetCurrentGame().Entities.Add(new HomingBoltEntity(MyPlayer, MyPlayer.Pos, mousePos, GetCurrentGame())
            {
                Speed = 1.0f,
                Color = "255, 255, 255",
                Size = size,
                TicksUntilDeletion = 90,
                Damage = _damage,
                Knockback = _knockback,
            });
            GoOnCooldown();
        }
    }
    public class HomingBoltEntity : Entity
    {
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public float Speed { get; set; }
        public int TicksUntilDeletion { get { return _ticksUntilDeletion; } set { _ticksUntilDeletion = value; _ticksUntilDeletionMax = value; } }
        private float _rotationSpeed = .075f;
        private Player CurrentTarget { get; set; }
        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
        private Game _game;
        private float _angle = 0;
        public HomingBoltEntity(Player owner, Vector2 startPos, Vector2 mousePos, Game game) : base(owner)
        {
            _game = game;
            Pos = startPos;
            ReTarget(mousePos);
        }
        public override void ReTarget(Vector2 pos)
        {
            CurrentTarget = FindClosestPlayer(pos);
            _angle = GetAngleToTarget();
            Vector2 dir = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle));
            Pos += dir * Speed * 2;
        }

        public override bool OnCollision(Player other)
        {
            if (_ticksUntilDeletionMax - _ticksUntilDeletion < 5 && other == MyCollider.Owner)
                return false;
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.PreviousPos - MyCollider.PreviousPos).Normalized(), Knockback);
            return true;
        }


        public override bool Update()
        {
            // maybe find new target
            if (_ticksUntilDeletionMax - _ticksUntilDeletion > 20)
            {
                CurrentTarget = FindClosestPlayer(Pos);
            }
            // rotate towards new angle
            var tempAngle = GetAngleToTarget();
            float diff = DeltaAngle(_angle, tempAngle);
            _angle += Math.Sign(diff) * Math.Min(Math.Abs(diff), _rotationSpeed);
            // move
            Vector2 dir = new Vector2(MathF.Cos(_angle), MathF.Sin(_angle));
            Pos += dir * Speed;

            _ticksUntilDeletion--;
            return _ticksUntilDeletion < 0;
        }

        private Player FindClosestPlayer(Vector2 pos)
        {
            Player p = _game.Players[0];
            float closestDist = (pos - _game.Players[0].Pos).LengthSqr();
            if (p.IsDead) // we would prefere not to fly towards a dead person
                closestDist = float.MaxValue;
            for (int i = 1; i < _game.Players.Count; i++)
            {
                if (_game.Players[i].IsDead)
                    continue;
                float dst = (pos - _game.Players[i].Pos).LengthSqr();
                if (dst < closestDist)
                {
                    closestDist = dst;
                    p = _game.Players[i];
                }
            }
            return p;
        }
        private float GetAngleToTarget()
        {
            Vector2 dir = CurrentTarget.Pos - Pos;
            float angle = MathF.Atan2(dir.y, dir.x);
            return angle;
        }
        public static float DeltaAngle(float current, float target)
        {
            float diff = (target - current) % (MathF.PI * 2);
            if (diff >= MathF.PI)
            {
                diff -= MathF.PI * 2;
            }
            else if (diff < -MathF.PI)
            {
                diff += MathF.PI * 2;
            }
            return diff;
        }
    }
}
