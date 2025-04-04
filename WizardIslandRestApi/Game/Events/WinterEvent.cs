using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Game.Events
{
    public class WinterEvent : EventBase
    {
        private const int frostFieldCount = 3;
        List<FrostFieldEntity> frostFieldEntities = new List<FrostFieldEntity>();
        public WinterEvent(Game game) : base(game)
        {
            Name = "Winter is here";
        }


        public override void Start()
        {
            Random r = new Random();
            float mapRadius = _game.GameMap.CircleRadius;
            Vector2 mapMiddle = _game.GameMap.GroundMiddle;
            for (int i = 0; i < frostFieldCount; i++)
            {
                float angle = (float)(r.NextDouble() * Math.PI * 2);
                float dist = (float)r.NextDouble() * mapRadius;
                Vector2 pos = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist + mapMiddle;
                frostFieldEntities.Add(new FrostFieldEntity(null, pos)
                {
                    TicksUntilDeletion = 999999,
                    Color = "100, 255, 100",
                    Size = 7.5f,
                });
            }
            for (int i = 0; i < frostFieldEntities.Count; i++)
                _game.Entities.Add(frostFieldEntities[i]);
        }
        public override void End()
        {
            for (int i = 0; i < frostFieldEntities.Count; i++)
                frostFieldEntities[i].TicksUntilDeletion = 0;
        }


        public override void EarlyUpdate()
        {
        }
        public override void LateUpdate()
        {
        }
    }
}
