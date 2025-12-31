namespace WizardIslandRestApi.Game.Spells.ExtraEntities
{
    public class WaitToDoSomethingEntity : Entity
    {
        public int TicksToWait { get; set; }
        public Action ActionAfterWaiting { get; set; }
        public WaitToDoSomethingEntity(int ticksToWait, Action actionAfterWaiting, Player owner = null) : base(owner)
        {
            TicksToWait = ticksToWait;
            ActionAfterWaiting = actionAfterWaiting;
        }

        public override bool OnCollision(Player other)
        { return false; }

        public override void ReTarget(Vector2 pos)
        {}

        public override bool Update()
        {
            if (--TicksToWait < 0)
                ActionAfterWaiting();
            return TicksToWait < 0; // in case ActionAfterWaiting changes TicksToWait, we have to check it again
        }
    }
}
