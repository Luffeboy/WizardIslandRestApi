using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells
{
    public class FrostField : Spell
    {
        private float _range = 30;
        public override string Name => "Frost field";
        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;
        public FrostField(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            Vector2 dir = mousePos - MyPlayer.Pos;
            float len = dir.Length();
            if (len != 0)
            {
                dir.x /= len;
                dir.y /= len;
            }
            if (len > _range)
                len = _range;
            Vector2 pos = MyPlayer.Pos + dir * len;
            GetCurrentGame().Entities.Add(new FrostFieldEntity(MyPlayer, pos)
            {
                Color = "100, 255, 100",
                Size = 5,
                TicksUntilDeletion = 4 * Game._updatesPerSecond
            });
            GoOnCooldown();
        }
    }
    public class FrostFieldEntity : Entity
    {
        private Vector2 _pos;
        public int TicksUntilDeletion { get; set; }
        public FrostFieldEntity(Player owner, Vector2 pos) : base(owner)
        {
            _pos = pos;
            Pos = pos;
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            other.ApplyDebuff(new Slowed(other) { SpeedMultiplier = .1f, TicksTillRemoval = 3 });
            return false;
        }

        public override bool Update()
        {
            Pos = _pos;
            TicksUntilDeletion--;
            return TicksUntilDeletion < 0;
        }
    }
}
