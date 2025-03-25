namespace WizardIslandRestApi.Game.Spells
{
    public abstract class CantHitOwnerAtStartSpellEntity : Entity
    {
        protected int _ticksUntilDeletion;
        protected int _ticksUntilDeletionMax;
        public abstract int TicksUntillCanHitOwner { get; set; }
        public CantHitOwnerAtStartSpellEntity(Player owner, int ticksUntilDeletion) : base(owner)
        {
            SetTicksUntilDeletion(ticksUntilDeletion);
        }
        private void SetTicksUntilDeletion(int value)
        { _ticksUntilDeletion = value; _ticksUntilDeletionMax = value; }

        public override bool OnCollision(Player other)
        {
            if (_ticksUntilDeletionMax - _ticksUntilDeletion < 5 && other == MyCollider.Owner)
                return false;
            return HitPlayer(other);
        }
        protected abstract bool HitPlayer(Player other);
    }
}
