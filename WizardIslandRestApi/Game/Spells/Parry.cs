namespace WizardIslandRestApi.Game.Spells
{
    public class Parry : Spell
    {
        public Parry(Player player) : base(player)
        {
        }

        public override int CooldownMax { get; protected set; } = 6 * Game._updatesPerSecond;

        public override void OnCast(Vector2 mousePos)
        {
            Vector2 dir = (mousePos - MyPlayer.Pos).Normalized();
            float size = 2;
            GetCurrentGame().Entities.Add(new ParryEntity(MyPlayer)
            {
                Pos = MyPlayer.Pos + dir * (size*.5f + MyPlayer.Size),
                Size = size,
                Color = "255,255,255",
                EntityId = "Parry",
            });
            GoOnCooldown();
        }
    }
    public class ParryEntity : Entity
    {
        public int TicksUntilDeletion { get; set; } = 4;
        public ParryEntity(Player owner) : base(owner)
        {
            Height = EntityHeight.Ground;
        }

        public override bool OnCollision(Player other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {

        }
        public override bool OnCollision(Entity other)
        {
            if (other.MyCollider.Owner == null || other.MyCollider.Owner == MyCollider.Owner)
                return false; // created by an event, or smt.
            if (base.OnCollision(other))
            {
                var newTarget = other.MyCollider.Owner.Pos;
                other.MyCollider.Owner = MyCollider.Owner;
                other.ReTarget(newTarget);
            }
            return false;
        }

        public override bool Update()
        {
            return TicksUntilDeletion-- < 0;
        }
    }
}
