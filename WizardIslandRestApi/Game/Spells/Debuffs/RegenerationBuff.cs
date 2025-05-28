namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class RegenerationBuff : DebuffBase
    {
        private int _ticksTilRemoval;
        private int _regenerationAmount;
        private int _ticksBetweenRegen;
        private int _ticksBetweenRegenCounter = 1;

        public RegenerationBuff(Player player, int ticksTilRemoval, int regenerationAmount, int ticksBetweenRegen) : base(player)
        {
            _ticksTilRemoval = ticksTilRemoval;
            _regenerationAmount = regenerationAmount;
            _ticksBetweenRegen = ticksBetweenRegen;
        }

        public override void OnApply()
        {
        }

        public override void OnRemove()
        {
        }

        public override bool Update()
        {
            if (++_ticksBetweenRegenCounter > _ticksBetweenRegen)
            {
                _player.Heal(_regenerationAmount);
                _ticksBetweenRegenCounter -= _ticksBetweenRegen;
            }
            return --_ticksTilRemoval < 0;
        }

        public override string ToString()
        {
            return "Regeneration";
        }
    }
}
