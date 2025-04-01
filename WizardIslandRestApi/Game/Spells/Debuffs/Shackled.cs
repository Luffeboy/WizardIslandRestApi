namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class Shackled : DebuffBase
    {
        //public float _playerSpeedBefore;
        public int TicksTillRemoval { get; set; }
        public Shackled(Player player) : base(player)
        {
        }
        public override void OnApply()
        {
            //_playerSpeedBefore = _player.Stats.Speed;
            _player.Stats.SpeedMultiplier *= .0001f;
        }
        public override void OnRemove()
        {
            _player.Stats.SpeedMultiplier /= .0001f;
        }

        public override bool Update()
        {
            _player.Vel = new Vector2(0, 0);
            TicksTillRemoval--;
            return TicksTillRemoval < 0;
        }
    }
}
