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
        public const float LavaDamage = 1.0f;
        public int AllowedSpellCount { get; private set; } = 1;
        public const int _updatesPerSecond = 30;
        private const int _gameMaxLengthMs = 5 * 60 * 1000; // 5 min
        //                               ms     fps
        private const int _sleepTimeMs = 1000 / _updatesPerSecond; // does not account for update time
        private const int _gameDuration = 5 * 60 * _updatesPerSecond;
        // unused, maybe...
        private DateTime _gameCreated;
        private DateTime _gameStarted;
        private DateTime _gameWillEnd;

        public Map GameMap { get; } = new Map();
        public int Id { get; private set; } // game id, for the GameManager
        private int _nextPlayerId;
        public int GameTick { get; private set; } = 0;
        public Dictionary<int, Player> Players { get; private set; } = new Dictionary<int, Player>();
        public List<Entity> Entities { get; private set; } = new List<Entity>();
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
                    if (GameTick >= _gameDuration)
                        CurrentState = GameState.Ended;
                }
                SleepBetweenUpdates();

            }
            GameTick = 0;
            while (CurrentState == GameState.Started)
            {
                lock (this)
                {
                    Update();
                    GameTick++;
                    if (GameTick >= _gameDuration * 60)
                        CurrentState = GameState.Ended;
                }
                SleepBetweenUpdates();
            }
        }

        public Player? AddPlayer(int[] spells)
        {
            if (CurrentState != GameState.Joinable)
                return null;
            int id = _nextPlayerId++;
            Player p = new Player(id, this, spells);
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
            // update players
            foreach (Player player in Players.Values)
                player.Update();
            // update entities, and check if they hit a player
            for (int i = 0; i < Entities.Count; i++)
            {
                // update entities
                if (Entities[i].Update())
                {
                    Entities.RemoveAt(i);
                    i--;
                    continue;
                }
                // check collision with players
                for (int j = 0; j < Players.Count; j++)
                {
                    if (!Players[j].IsDead && Entities[i].MyCollider.CheckCollision(Players[j].MyCollider))
                    {
                        if (Entities[i].OnCollision(Players[j]))
                        {
                            Entities.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                }
            }
            // check for entity collisions, with each other
            for (int i = 0; i < Entities.Count; i++)
            {
                for (int j = i+1; j < Entities.Count; j++)
                {
                    if (Entities[i].MyCollider.CheckCollision(Entities[j].MyCollider))
                    {
                        bool deleteFirst = Entities[i].OnCollision(Entities[j]);
                        bool deleteSecond = Entities[j].OnCollision(Entities[i]);
                        if (deleteSecond) Entities.RemoveAt(j); // j should be greater than i, so we delete it first
                        if (deleteFirst) { Entities.RemoveAt(i); i--; }
                        break;
                    }
                }
            }
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
