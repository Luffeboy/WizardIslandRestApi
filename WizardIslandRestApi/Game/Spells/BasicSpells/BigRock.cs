namespace WizardIslandRestApi.Game.Spells.BasicSpells
{
    public class BigRock : Spell
    {
        public override string Name => "Big rock";
        public BigRock(Player player) : base(player)
        {
            Tags.Add(SpellTags.Static);
            Tags.Add(SpellTags.Summon);
            StandardStats.Range = 20;
            StandardStats.SummonLifetime = 5 * Game._updatesPerSecond;
            StandardStats.Size = 3;

            Tags.Add(SpellTags.CreateEnvironment);
            Tags.Add(SpellTags.Summon);
            Tags.Add(SpellTags.Zone);
            Tags.Add(SpellTags.Static);
        }

        public override int CooldownMax { get; protected set; } = (int)(6.5f * Game._updatesPerSecond);

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 dir = mousePos - startPos;
            if (dir.LengthSqr() > StandardStats.Range * StandardStats.Range)
                dir = dir.Normalized() * StandardStats.Range;
            int quantity = StandardStats.OtherStatsInt[SpellSpecificStats.SummonQuantity];
            Random r = new Random();
            Vector2 firstPos = startPos + dir;
            Vector2 pos = firstPos;
            for (int i = 0; i < quantity; i++)
            {
                GetCurrentGame().Entities.Add(new BigRockEntity(StandardStats.SummonLifetime, pos)
                {
                    Size = StandardStats.Size
                });
                pos = firstPos + new Vector2((float)(r.NextDouble() * 2 - 1), (float)(r.NextDouble() * 2 - 1)) * StandardStats.Size / 2;
            }
            GoOnCooldown();
        }
    }
    public class BigRockEntity : Entity
    {
        protected int _ticksUntilDeletion;
        public BigRockEntity(int ticksUntilDeletion, Vector2 startPos) : base(null, startPos)
        {
            Color = "75,45,10";
            _ticksUntilDeletion = ticksUntilDeletion;
            EntityId = "Rock";
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            Vector2 dir = (Pos - other.MyCollider.Pos).Normalized();
            other.TeleportTo(Pos - dir * (Size + other.Size + .25f));
            other.Vel = other.Vel * (1 - dir.Dot(other.Vel.Normalized()));
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {}

        public override bool Update()
        {
            return --_ticksUntilDeletion < 0;
        }
    }
}
