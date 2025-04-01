using System.Xml.Linq;

namespace WizardIslandRestApi.Game.Events
{
    public class RisingTideEvent : EventBase
    {
        private float _maxCircleRadiusDelta;
        private float _circleRadius;
        private float _minGroundRadius = 5;
        public RisingTideEvent(Game game) : base(game)
        {
            Name = "Rising tide";
        }

        public override void EarlyUpdate()
        {
            float eventProgress = (float)_game.TicksTillNextEvent / (float)_game.TicksTillNextEventMax;
            // make it -1 to 1, then 1 -> 0 -> 1 (0% -> 50% -> 100% event progress)
            eventProgress = MathF.Abs(eventProgress * 2 - 1);
            _game.GameMap.CircleRadius = eventProgress * _maxCircleRadiusDelta + _game.GameMap.CircleInnerRadius + _minGroundRadius;
        }

        public override void End()
        {
            _game.GameMap.CircleRadius = _circleRadius;
        }

        public override void LateUpdate()
        {
        }

        public override void Start()
        {
            _circleRadius = _game.GameMap.CircleRadius;
            _maxCircleRadiusDelta = _circleRadius - _game.GameMap.CircleInnerRadius - _minGroundRadius;
        }
    }
}
