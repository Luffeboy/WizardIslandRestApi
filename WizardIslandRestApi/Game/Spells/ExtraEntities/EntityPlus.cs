namespace WizardIslandRestApi.Game.Spells.ExtraEntities
{
    public abstract class EntityPlus : CantHitOwnerAtStartSpellEntity
    {
        public override int TicksUntillCanHitOwner { get; set; } = 5;
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public Vector2 Dir {  get; set; }
        protected EntityPlus(Player owner, int ticksUntilDeletion, Vector2 startPos) : base(owner, ticksUntilDeletion, startPos)
        {
        }

        public override void ReTarget(Vector2 pos)
        {
            _ticksUntilDeletion = _ticksUntilDeletionMax;
            Dir = (pos - Pos).Normalized();
        }
    }
}
