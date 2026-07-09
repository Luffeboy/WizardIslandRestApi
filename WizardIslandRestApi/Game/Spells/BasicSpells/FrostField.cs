using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class FrostField : Spell
    {
        public override string Name => "Frost field";
        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;
        public FrostField(Player player) : base(player)
        {
            Tags.Add(SpellTags.Debuff);
            Tags.Add(SpellTags.Static);
            Tags.Add(SpellTags.Zone);
            Tags.Add(SpellTags.LongRange);
            Tags.Add(SpellTags.ShortCooldown);
            StandardStats.Size = 7;
            StandardStats.Range = 30;
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            Vector2 dir = mousePos - pos;
            float len = dir.Length();
            if (len != 0)
            {
                dir.x /= len;
                dir.y /= len;
            }
            if (len > StandardStats.Range)
                len = StandardStats.Range;
            pos = pos + dir * len;
            GetCurrentGame().Entities.Add(new FrostFieldEntity(MyPlayer, pos)
            {
                Color = "100, 255, 100",
                Size = StandardStats.Size,
                TicksUntilDeletion = 4 * Game._updatesPerSecond
            });
            GoOnCooldown();
        }
    }
    public class FrostFieldEntity : Entity
    {
        private Vector2 _pos;
        public int TicksUntilDeletion { get; set; }
        public float SlowAmount { get; set; } = .1f;
        public FrostFieldEntity(Player owner, Vector2 pos) : base(owner, pos)
        {
            _pos = pos;
            Height = EntityHeight.Ground;
            EntityId = "FrostField";
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            other.ApplyDebuff(new Slowed(other) { SpeedMultiplier = SlowAmount, TicksTillRemoval = 3 });
            return false;
        }

        public override bool Update()
        {
            //Pos = _pos;
            TicksUntilDeletion--;
            return TicksUntilDeletion < 0;
        }
        public override void ReTarget(Vector2 pos) { }
    }
}
