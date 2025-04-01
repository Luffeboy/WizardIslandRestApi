namespace WizardIslandRestApi.Game.Events
{
    public class UltraRapidFire : EventBase
    {
        float _prevCooldownMultiplier;
        public UltraRapidFire(Game game) : base(game)
        {
        }

        public override void EarlyUpdate()
        {
        }

        public override void End()
        {
            _game.GameModifiers.CooldownMultiplier = _prevCooldownMultiplier;
        }

        public override void LateUpdate()
        {
        }

        public override void Start()
        {
            _prevCooldownMultiplier = _game.GameModifiers.CooldownMultiplier;
            _game.GameModifiers.CooldownMultiplier = .25f;
        }
    }
}
