namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class LavaDebuff : DebuffBase
    {
        private int _ticksUntilRemoval;
        public LavaDebuff(Player player, int ticksUntilRemoval) : base(player)
        {
            _ticksUntilRemoval = ticksUntilRemoval;
        }

        public override void OnApply()
        {
            throw new NotImplementedException();
        }

        public override void OnRemove()
        {
            throw new NotImplementedException();
        }

        public override bool Update()
        {
            _player.IsInLava = true;
            return --_ticksUntilRemoval < 0;
        }
    }
}
