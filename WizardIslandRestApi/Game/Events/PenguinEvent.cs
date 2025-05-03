using WizardIslandRestApi.Game.Spells.ExtraEntities;

namespace WizardIslandRestApi.Game.Events
{
    public class PenguinEvent : EventBase
    {
        private int _penguinCount = 10;
        private List<PenguinEntity> _penguins = new List<PenguinEntity>();
        public PenguinEvent(Game game) : base(game)
        {
            Name = "Penguin invasion";
        }

        public override void EarlyUpdate()
        {
        }

        public override void End()
        {
            // the penguins should expire by themselves.
        }

        public override void LateUpdate()
        {
        }

        public override void Start()
        {
            Random r = new Random();
            for (int i = 0; i < _penguinCount; i++)
            {
                float angle = r.NextSingle() * MathF.PI * 2;
                float dist = (_game.GameMap.CircleRadius - _game.GameMap.CircleInnerRadius) * r.NextSingle() + _game.GameMap.CircleInnerRadius;
                var penguin = new PenguinEntity(null, _game.TicksTillNextEventMax, _game.GameMap.GroundMiddle + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist, (player, pen) =>
                {
                    for (int j = 0; j < _penguins.Count; j++)
                    {
                        if (_penguins[j] == pen)
                        {
                            _penguins.RemoveAt(j);
                            j--;
                            continue;
                        }
                        _penguins[j].TargetPlayer = player;
                    }
                }, _game)
                {
                    Size = 1.0f,
                    Damage = 10,
                    Knockback = 2,
                    Speed = .2f,
                };
                _penguins.Add(penguin);
                _game.Entities.Add(penguin);
            }
        }
    }
}
