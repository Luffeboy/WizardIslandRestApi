using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells.Movement
{
    public class BullCharge : Spell
    {
        public override string Name { get { return "Bull-charge"; } }
        public override SpellType Type { get; set; } = SpellType.Movement;
        private float _damage = 5;
        private float _knockback = 1.5f;
        private float _range = 30.0f;
        public override int CooldownMax { get; protected set; } = (int)(13 * Game._updatesPerSecond);
        public BullCharge(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            var dir = (mousePos - MyPlayer.Pos);
            if (dir.LengthSqr() > _range * _range)
            {
                dir = dir.Normalized() * _range;
            }
            int ticksUntillDeletion = (int)(dir.Length() * .3f);
            MyPlayer.Vel = new Vector2(0, 0);
            MyPlayer.ApplyDebuff(new Shackled(MyPlayer));
            MyPlayer.TargetPos = mousePos;
            GetCurrentGame().Entities.Add(new BullChargeEntity(MyPlayer, ticksUntillDeletion) 
            {
                StartPos = MyPlayer.Pos,
                EndPos = MyPlayer.Pos + dir,
                Knockback = _knockback,
                Damage = _damage,
                Color = "0,0,0"
            });
            GoOnCooldown();
        }
    }

    class BullChargeEntity : Entity
    {
        List<Player> _hasHitPlayers = new List<Player>();
        private int _ticksUntilDeletionMax;
        private int _ticksUntilDeletion;
        public Vector2 StartPos { get; set; }
        public Vector2 EndPos { get; set; }
        public float Knockback { get; set; }
        public float Damage { get; set; }
        public BullChargeEntity(Player owner, int ticksUntilDeletion) : base(owner)
        {
            _ticksUntilDeletion = 0;
            _ticksUntilDeletionMax = ticksUntilDeletion;
            _hasHitPlayers.Add(owner);
            Size = owner.Size;
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            if (_hasHitPlayers.Contains(other))
                return false;
            _hasHitPlayers.Add(other);
            Vector2 knockbackDir = other.MyCollider.PreviousPos - MyCollider.PreviousPos;
            other.ApplyKnockback(knockbackDir.Normalized(), Knockback);
            other.TakeDamage(Damage, MyCollider.Owner);
            return false;
        }

        public override bool Update()
        {
            _ticksUntilDeletion++;
            float t = (float)_ticksUntilDeletion / (float)_ticksUntilDeletionMax;
            Vector2 pos = Vector2.Lerp(StartPos, EndPos, t);
            MyCollider.Pos = pos;
            MyCollider.Owner.Pos = pos;
            if (_ticksUntilDeletion > _ticksUntilDeletionMax-1)
            {
                Size *= 3;
            }
            return _ticksUntilDeletion > _ticksUntilDeletionMax;
        }

        public override void ReTarget(Vector2 pos)
        {
        }
    }
}
