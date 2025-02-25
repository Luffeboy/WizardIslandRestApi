namespace WizardIslandRestApi.Game
{
    public enum GameState
    {
        Joinable,
        Started,
        Ended
    }
    public class Game
    {
        public const int _updatesPerSecond = 30;
        private const int _gameMaxLengthMs = 5 * 60 * 1000; // 5 min
        //                               ms     fps
        private const int _sleepTimeMs = 1000 / _updatesPerSecond; // does not account for update time
        private DateTime _gameCreated;
        private DateTime _gameStarted;
        private DateTime _gameWillEnd;
        public Map GameMap { get; } = new Map();
        public int Id { get; private set; } // game id, for the GameManager
        private int _nextPlayerId;
        public int GameTick { get; private set; } = 0;
        public Dictionary<int, Player> Players { get; private set; } = new Dictionary<int, Player>();
        public bool CanJoin { get { return CurrentState == GameState.Joinable; } }
        public GameState CurrentState { get; private set; }
        public float GlobalDamageMultiplier { get; private set; } = 0.0f;

        public Game(int id) 
        {
            Id = id;
            _gameCreated = DateTime.Now;
            CurrentState = GameState.Joinable;
        }
        public void GameLoop()
        {
            while (CurrentState == GameState.Joinable)
            {
                // waiting for players
                lock (this) 
                {
                    LobbyUpdate();
                    GameTick++;
                    if (GameTick >= 4000)
                        CurrentState = GameState.Ended; // temp
                }
                SleepBetweenUpdates();

            }
            while (CurrentState == GameState.Started)
            {
                lock (this)
                {
                    Update();
                    GameTick++;
                }
                SleepBetweenUpdates();
            }
        }

        public Player AddPlayer()
        {
            if (CurrentState != GameState.Joinable)
                return null;
            int id = _nextPlayerId++;
            Player p = new Player(id, Id);
            Players.Add(id, p);
            return p;
        }

        private void LobbyUpdate()
        {
            foreach (Player player in Players.Values)
                player.Update();
        }
        private void Update() 
        { 
        }
        public void StartGame()
        {
            _gameStarted = DateTime.Now;
            CurrentState = GameState.Started;
            // reset all players
            for (int i = 0; i < Players.Count; i++) 
            {
                Players[i].Reset();
            }
            GlobalDamageMultiplier = 1;
        }

        public void EndGame()
        {
            CurrentState = GameState.Ended;
            GameManager.Instance.DeleteGame(Id);
        }


        private void SleepBetweenUpdates()
        {
            Thread.Sleep(_sleepTimeMs);
        }
    }
}
