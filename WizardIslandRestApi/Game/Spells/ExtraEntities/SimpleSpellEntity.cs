namespace WizardIslandRestApi.Game.Spells.ExtraEntities
{
    public class SimpleSpellEntity : Entity
    {
        public SimpleSpellEntity(Player owner, Vector2 startPos) : base(owner, startPos)
        {
        }

        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
        public Vector2 Dir { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public List<string> EntityIdsToIgnore { get; set; } = [];

        /// <summary>
        /// Gets set to false, when the spell has moved out of the owners hitbox once.
        /// Can be set to false, if this entity should be able to hit the owner directly after being created.
        /// </summary>
        public bool IgnoreHitOnOwnerOnSpawn = true;
        protected bool _hitOwnerLastUpdate = true;
        public int TicksUntilDeletion
        {
            get { return _ticksUntilDeletion; }
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
            }
        }

        public override bool OnCollision(Entity other)
        {
            if (EntityIdsToIgnore.Contains(other.EntityId))
                return false;
            return base.OnCollision(other);
        }

        public override void ReTarget(Vector2 pos)
        {
            _ticksUntilDeletion = _ticksUntilDeletionMax;
            Dir = (pos - Pos).Normalized();
        }

        public override bool OnCollision(Player other)
        {
            if (IgnoreHitOnOwnerOnSpawn && other == MyCollider.Owner)
            {
                _hitOwnerLastUpdate = true;
                return false;
            }
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.Pos - (Pos - Dir * Speed * 5)).Normalized(), Knockback);

            return true;
        }

        public override bool Update()
        {
            Pos += Dir * Speed;
            _ticksUntilDeletion--;
            MyCollider.Pos = Pos;
            IgnoreHitOnOwnerOnSpawn = IgnoreHitOnOwnerOnSpawn && _hitOwnerLastUpdate;
            _hitOwnerLastUpdate = false;
            return _ticksUntilDeletion <= 0;
        }
    }
}
