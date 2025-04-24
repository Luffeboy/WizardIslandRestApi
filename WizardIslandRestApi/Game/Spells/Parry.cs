namespace WizardIslandRestApi.Game.Spells
{
    public class Parry : Spell
    {
        public Parry(Player player) : base(player)
        {
        }

        public override int CooldownMax { get; protected set; } = 3 * Game._updatesPerSecond;

        public override void OnCast(Vector2 mousePos)
        {
            GetCurrentGame().Entities.Add(new ParryEntity(MyPlayer)
            {

            });
        }
    }
    public class ParryEntity : Entity
    {
        public ParryEntity(Player owner) : base(owner)
        {
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
            if (base.OnCollision(other))
            {

            }
            return false;
        }

        public override bool Update()
        {
            throw new NotImplementedException();
        }
    }
}
