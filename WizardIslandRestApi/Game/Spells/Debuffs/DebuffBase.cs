namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public abstract class DebuffBase
    {
        protected Player _player;
        public DebuffBase(Player player) { _player = player; }
        public abstract void OnApply();
        //public abstract void OnReApply(DebuffBase theNewOne);
        public abstract bool Update();
        public abstract void OnRemove();
    }
}
