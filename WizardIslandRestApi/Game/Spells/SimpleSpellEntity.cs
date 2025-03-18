namespace WizardIslandRestApi.Game.Spells
{
    public class SimpleSpellEntity : Entity
    {
        public SimpleSpellEntity(Player owner) : base(owner)
        {
        }

        public Vector2 Dir { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public int TicksUntilDeletion { get; set; }
        public override bool OnCollision(Entity other)
        {
            return true;
        }

        public override bool OnCollision(Player other)
        {
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.PreviousPos - MyCollider.PreviousPos).Normalized(), Knockback);

            return true;
        }

        public override bool Update()
        {
            Pos += Dir * Speed;
            TicksUntilDeletion--;
            MyCollider.Pos = Pos;
            return TicksUntilDeletion <= 0;
        }
    }
}
