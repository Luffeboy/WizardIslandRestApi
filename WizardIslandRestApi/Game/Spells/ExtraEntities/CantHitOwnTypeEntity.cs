namespace WizardIslandRestApi.Game.Spells.ExtraEntities
{
    public abstract class CantHitOwnTypeEntity : CantHitOwnerAtStartSpellEntity
    {
        public CantHitOwnTypeEntity(Player owner, int ticksUntilDeletion, Vector2 startPos) : base(owner, ticksUntilDeletion, startPos)
        {
        }
        public override bool OnCollision(Entity other)
        {
            if (!base.OnCollision(other))
                return false;
            if (other.GetType() == this.GetType())
                return false;
            return true;
        }
    }
}
