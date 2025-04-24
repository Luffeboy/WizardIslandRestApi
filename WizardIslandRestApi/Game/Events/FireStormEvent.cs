using System.Drawing;
using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Events
{
    public class FireStormEvent : EventBase
    {
        private int _ticksTillNextFireballMax = 10;
        private int _ticksTillNextFireball;
        private Random _random = new Random();
        public FireStormEvent(Game game) : base(game)
        {
            Name = "Fire storm";
            _ticksTillNextFireball = 0;
        }

        public override void EarlyUpdate()
        {
            _ticksTillNextFireball--;
            if (_ticksTillNextFireball < 0)
            {
                float angle = (float)(_random.NextDouble() * Math.PI * 2);
                _game.Entities.Add(new SimpleSpellEntity(null)
                {
                    Pos = _game.GameMap.GroundMiddle,
                    Dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle)),
                    Speed = 2f,
                    Color = "255, 0, 0",
                    Size = .5f,
                    TicksUntilDeletion = 90,
                    Damage = 5,
                    Knockback = 1.5f,
                    EntityId = "FireBall",
                });
                _ticksTillNextFireball = _ticksTillNextFireballMax;
            }
        }

        public override void End()
        {
        }

        public override void LateUpdate()
        {
        }

        public override void Start()
        {
        }
    }
}
