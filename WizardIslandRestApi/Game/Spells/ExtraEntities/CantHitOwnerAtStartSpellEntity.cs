namespace WizardIslandRestApi.Game.Spells.ExtraEntities
{
    public abstract class CantHitOwnerAtStartSpellEntity : Entity
    {
        protected int _ticksUntilDeletion;
        protected int _ticksUntilDeletionMax;

        /// <summary>
        /// Gets set to false, when the spell has moved out of the owners hitbox once.
        /// Can be set to false, if this entity should be able to hit the owner directly after being created.
        /// </summary>
        public bool IgnoreHitOnOwnerOnSpawn = true;
        private bool _hitOwnerLastUpdate = true;
        public CantHitOwnerAtStartSpellEntity(Player owner, int ticksUntilDeletion, Vector2 startPos) : base(owner, startPos)
        {
            SetTicksUntilDeletion(ticksUntilDeletion);
        }
        private void SetTicksUntilDeletion(int value)
        { _ticksUntilDeletion = value; _ticksUntilDeletionMax = value; }

        public override bool OnCollision(Player other)
        {
            if (IgnoreHitOnOwnerOnSpawn && other == MyCollider.Owner)
            {
                _hitOwnerLastUpdate = true;
                return false;
            }
            return HitPlayer(other);
        }

        public override bool Update()
        {
            IgnoreHitOnOwnerOnSpawn = IgnoreHitOnOwnerOnSpawn && _hitOwnerLastUpdate;
            _hitOwnerLastUpdate = false;
            return --_ticksUntilDeletion <= 0;
        }
        protected abstract bool HitPlayer(Player other);
    }
}
