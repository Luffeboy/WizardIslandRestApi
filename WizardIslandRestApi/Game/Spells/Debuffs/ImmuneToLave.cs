namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class ImmuneToLave : DebuffBase
    {
        private int _ticksUntilRemoval;
        public ImmuneToLave(Player player, int ticksUntilRemoval) : base(player)
        {
            _ticksUntilRemoval = ticksUntilRemoval;
        }

        public override void OnApply()
        {
            _player.ImmuneToLavaDamage = true;
        }

        public override void OnRemove()
        {
            _player.ImmuneToLavaDamage = false;
        }

        public override bool Update()
        {
            return --_ticksUntilRemoval < 0;
        }
    }
}
