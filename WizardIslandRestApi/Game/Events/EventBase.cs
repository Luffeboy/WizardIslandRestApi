namespace WizardIslandRestApi.Game.Events
{
    public abstract class EventBase
    {
        public string Name { get; set; } = "No event name...";
        protected Game _game;
        public EventBase(Game game)
        {
            _game = game;
        }
        private static Random _random = new Random();
        private static int ___temp = 0;
        private static Func<Game, EventBase>[] _gameFunc = new Func<Game, EventBase>[]
        {
            (game) => new NoEvent(game),
            (game) => new FireStormEvent(game),
            (game) => new RisingTideEvent(game),
            (game) => new UltraRapidFire(game),
        };
        public static EventBase GetRandomEvent(Game game)
        {
            return _gameFunc[(++___temp) % _gameFunc.Length](game);
            //return _gameFunc[_random.Next(_gameFunc.Length)](game);
        }

        /// <summary>
        /// when the event starts, this function is called once
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// This function is called, before players and entities update function
        /// </summary>
        public abstract void EarlyUpdate();
        /// <summary>
        /// This function is called, after players and entities update function
        /// </summary>
        public abstract void LateUpdate();
        /// <summary>
        /// When the event ends
        /// </summary>
        public abstract void End();
    }
}
