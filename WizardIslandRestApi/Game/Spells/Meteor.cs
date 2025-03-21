using WizardIslandRestApi.Game.Physics;

namespace WizardIslandRestApi.Game.Spells
{
    public class Meteor : Spell
    {
        public override string Name { get { return "Meteor"; } }
        public override int CooldownMax { get; protected set; } = (int)(8.0f * Game._updatesPerSecond);
        private float _damage = 5;
        private float _knockbackMin = 1.3f;
        private float _knockbackMax = 2.5f;
        private int _fallTime = (int)(1.0f * Game._updatesPerSecond);
        public Meteor(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            var dir = (mousePos - MyPlayer.Pos).Normalized();
            float size = 2.5f;
            GetCurrentGame().Entities.Add(new MeteorEntity(MyPlayer)
            {
                Pos = mousePos,
                Color = "50, 50, 50",
                Size = size,
                FallTime = _fallTime,
                Damage = _damage,
                KnockbackMin = _knockbackMin,
                KnockbackMax = _knockbackMax,
            });
            GoOnCooldown();
        }
    }
    public class MeteorEntity : Entity
    {
        public float Damage { get; set; }
        public float KnockbackMin { get; set; }
        public float KnockbackMax { get; set; }
        public int FallTime { get; set; }
        List<Player> _hitPlayers = new List<Player>();
        private Player _player;
        public MeteorEntity(Player owner) : base(owner)
        {
            _player = owner;
            MyCollider = null;
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            // can't hit the same enemy twice
            if (_hitPlayers.Contains(other))
                return false;
            _hitPlayers.Add(other);
            // deal damage
            other.TakeDamage(Damage, _player);
            // calculate knockback, between kbMin and kbMax
            Vector2 dir = (other.MyCollider.PreviousPos - Pos);
            float dist = dir.Length();
            float knockBackMultiplier = MathF.Max(1 - dist / Size, 0);
            float knockback = (KnockbackMax - KnockbackMin) * (knockBackMultiplier) + KnockbackMin;
            // if dist is 0, we could get a /0 error, we don't want that
            if (dist == 0)
                dist = 0.1f;
            other.ApplyKnockback(dir / dist, knockback);
            return false;
        }

        public override bool Update()
        {
            if (FallTime < -3)
                return true;
            if (FallTime <= 0)
            {
                MyCollider = new Physics.Collider() { Owner = _player, Pos = Pos, Size = Size };
                MyCollider.Pos = Pos; // to update prevpos, if used for something for some reason
                Color = "255, 0, 0";
            }
            FallTime--;
            return false;
        }
    }
}
