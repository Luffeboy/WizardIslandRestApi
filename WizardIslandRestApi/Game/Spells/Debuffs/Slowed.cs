namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class Slowed : DebuffBase
    {
        public float SpeedMultiplier { get; set; }
        public int TicksTillRemoval { get; set; }
        public Slowed(Player player) : base(player)
        {
        }

        public override void OnApply()
        {
            _player.Stats.SpeedMultiplier = SpeedMultiplier;
        }

        public override void OnRemove()
        {
            _player.Stats.SpeedMultiplier = 1.0f;
        }

        public override bool Update()
        {
            TicksTillRemoval--;
            return TicksTillRemoval < 0;
        }
    }
}
