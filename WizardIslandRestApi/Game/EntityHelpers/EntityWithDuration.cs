namespace WizardIslandRestApi.Game.Spells
{
    public abstract class EntityWithDuration : Entity
    {
        protected int _ticksUntilDeletion;
        protected int _ticksUntilDeletionMax;
        public int TicksUntilDeletion
        {
            get { return _ticksUntilDeletion; }
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax - 1;
            }
        }
        public EntityWithDuration(Player owner, Vector2 startPos) : base(owner, startPos)
        {
        }

        /// <summary>
        /// Returns true if the entity's duration has ended and it should be deleted.
        /// </summary>
        /// <returns></returns>
        public override bool Update()
        {
            return --_ticksUntilDeletion <= 0;
        }
    }
}
