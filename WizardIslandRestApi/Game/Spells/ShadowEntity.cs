namespace WizardIslandRestApi.Game.Spells
{
    public class ShadowEntity : Entity
    {
        private int _ticksUntilDeletion;
        private int _ticksUntilDeletionMax;
        public int TicksUntilDeletion
        {
            get { return _ticksUntilDeletion; }
            set
            {
                _ticksUntilDeletionMax = value;
                _ticksUntilDeletion = _ticksUntilDeletionMax;
            }
        }
        public ShadowEntity(Player owner = null) : base(owner)
        {
            Color = "30, 30, 30";
            Height = EntityHeight.Ground;
            EntityId = "Shadow";
        }

        public override bool OnCollision(Entity other)
        {
            return false;
        }

        public override bool OnCollision(Player other)
        {
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
        }

        public override bool Update()
        {
            return _ticksUntilDeletion-- < 0;
        }
    }
}
