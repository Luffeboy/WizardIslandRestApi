namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class CooldownMultiplierBuff : DebuffBase
    {
        public float CooldownMultiplier { get; set; }
        public int TicksTillRemoval { get; set; }
        public CooldownMultiplierBuff(Player player) : base(player)
        {
            Stackable = true;
        }

        public override void OnApply()
        {
            _player.Stats.CooldownMultiplier *= CooldownMultiplier;
        }

        public override void OnRemove()
        {
            _player.Stats.CooldownMultiplier /= CooldownMultiplier;
        }

        public override bool Update()
        {
            return --TicksTillRemoval < 0;
        }

        public override string ToString()
        {
            return "CooldownMultiplier";
        }
    }
}
