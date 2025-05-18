namespace WizardIslandRestApi.Game.Spells
{
    public class BigRock : Spell
    {
        private float _range = 20;
        private int _rockLifetime = 5 * Game._updatesPerSecond;
        public BigRock(Player player) : base(player)
        {
        }

        public override int CooldownMax { get; protected set; } = (int)(6.5f * Game._updatesPerSecond);

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 dir = mousePos - startPos;
            if (dir.LengthSqr() > _range * _range)
                dir = dir.Normalized() * _range;
            GetCurrentGame().Entities.Add(new BigRockEntity(_rockLifetime, startPos + dir)
            {
                Size = 3
            });
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
