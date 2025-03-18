namespace WizardIslandRestApi.Game.Spells.Debuffs
{
    public class Shackled : DebuffBase
    {
        public float _playerSpeedBefore;
        public int TicksTillRemoval { get; set; }
        public Shackled(Player player) : base(player)
        {
        }
        public override void OnApply()
        {
            _playerSpeedBefore = _player.Stats.Speed;
            _player.Stats.Speed = 0;
        }
        public override void OnRemove()
        {
            _player.Stats.Speed = _playerSpeedBefore;
        }

        public override bool Update()
        {
            TicksTillRemoval--;
            return TicksTillRemoval < 0;
        }
    }
}
