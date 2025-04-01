namespace WizardIslandRestApi.Game.Events
{
    public class NoEvent : EventBase
    {
        public NoEvent(Game game) : base(game)
        {
            Name = "No event currently";
        }

        public override void EarlyUpdate()
        {
        }

        public override void End()
        {
        }

        public override void LateUpdate()
        {
        }

        public override void Start()
        {
        }
    }
}
