using WizardIslandRestApi.Game;
using WizardIslandRestApi.Game.Events;
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
        //private static Func<Game, EventBase>[] _gameFunc = new Func<Game, EventBase>[]
        //{
        //    (game) => new NoEvent(game),
        //    (game) => new FireStormEvent(game),
        //    (game) => new RisingTideEvent(game),
        //    (game) => new UltraRapidFire(game),
        //    (game) => new WinterEvent(game),
        //    (game) => new BackToBasicsEvent(game),
        //};
        private static int MaxWeights { get { return _gameEventsAndWeights.Sum(eaw => eaw.Weight);  } }
        private static EventAndWeight[] _gameEventsAndWeights = new EventAndWeight[]
        {
            new EventAndWeight(5, (game) => new NoEvent(game)),
            new EventAndWeight(1, (game) => new FireStormEvent(game)),
            new EventAndWeight(2, (game) => new RisingTideEvent(game)),
            new EventAndWeight(1, (game) => new UltraRapidFire(game)),
            new EventAndWeight(1, (game) => new WinterEvent(game)),
            new EventAndWeight(1, (game) => new BackToBasicsEvent(game)),
        };
    //private static int ___temp = 0;
    public static EventBase GetRandomEvent(Game game)
        {
            //return _gameFunc[(++___temp) % _gameFunc.Length](game);
            //return _gameFunc[_random.Next(_gameFunc.Length)](game);
            int randomNumber = _random.Next(MaxWeights);
            int weight = 0;
            for (int i = 0; i < _gameEventsAndWeights.Length; i++)
                if (randomNumber < (weight += _gameEventsAndWeights[i].Weight))
                    return _gameEventsAndWeights[i].MyEvent(game);

            return _gameEventsAndWeights[_gameEventsAndWeights.Length - 1].MyEvent(game); // if all else fails
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

public class EventAndWeight
{
    public int Weight { get; set; }
    public Func<Game, EventBase> MyEvent { get; set; }
    public EventAndWeight(int weight, Func<Game, EventBase> myEvent)
    {
        Weight = weight;
        MyEvent = myEvent;
    }
}
