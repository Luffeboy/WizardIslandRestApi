namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public abstract class DebuffBase
    {
        public bool Stackable { get; set; } = false;
        public int Stacks { get; set; } = 0;
        protected Player _player;
        public DebuffBase(Player player) { _player = player; }
        /// <summary>
        /// This is called before "OnApply", where you get access to the debuff that is being removed.
        /// The default just adds the "stacks"
        /// </summary>
        /// <param name="debuff">The debuff being removed, this should always be of the same type as the class overriding this method.</param>
        public virtual void OnApplyWhereDebuffOfSameNameWasRemoved(DebuffBase debuff)
        {
            Stacks += debuff.Stacks;
        }
        public abstract void OnApply();
        public abstract bool Update();
        public abstract void OnRemove();

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
