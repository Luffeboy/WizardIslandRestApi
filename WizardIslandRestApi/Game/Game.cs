using WizardIslandRestApi.Game.Events;

namespace WizardIslandRestApi.Game
{
    public enum GameState
    {
        Joinable,
        Started,
        Ended
    }
    public class GameModifier
    {
        public float KnockbackMultiplier { get; set; } = 1;
        public float DamageMultiplier { get; set; } = 1;
        public float CooldownMultiplier { get; set; } = 1;
    }
    public class Game
    {
        public const float LavaDamage = 1.0f;
        public int AllowedSpellCount { get; private set; } = 5;
        public const int _updatesPerSecond = 30;
        private const int _gameMaxLengthMs = 5 * 60 * 1000; // 5 min
        //                               ms     fps
        private const int _sleepTimeMs = 1000 / _updatesPerSecond; // does not account for update time
        public const int _gameDuration = 5 * 60 * _updatesPerSecond;
        // unused, maybe...
        private DateTime _gameCreated;
        private DateTime _gameStarted;
        private DateTime _gameWillEnd;
        private long _lastUpdateTick;

        public Map GameMap { get; } = new Map();
        // event stuff
        public EventBase CurrentEvent { get; private set; }
        public EventBase NextEvent { get; private set; }
        public int TicksTillNextEventMax { get; private set; } = 30 * _updatesPerSecond;
        public int TicksTillNextEvent { get; private set; }

        public int Id { get; private set; } // game id, for the GameManager
        private int _nextPlayerId;
        public int GameTick { get; private set; } = 0;
        public Dictionary<int, Player> Players { get; private set; } = new Dictionary<int, Player>();
        public List<Entity> Entities { get; private set; } = new List<Entity>();
        public bool CanJoin { get { return CurrentState == GameState.Joinable; } }
        public GameState CurrentState { get; private set; }
        public float GlobalDamageMultiplier { get; private set; } = 0.0f;
        public GameModifier GameModifiers { get; private set; } = new GameModifier();

        public Game(int id) 
        {
            Id = id;
            _gameCreated = DateTime.Now;
            CurrentState = GameState.Joinable;
            CurrentEvent = new NoEvent(this);
            NextEvent = new NoEvent(this);
            TicksTillNextEvent = TicksTillNextEventMax;
            GameModifiers.DamageMultiplier = 0;
        }
        public void GameLoop()
        {
            _lastUpdateTick = DateTime.Now.Ticks;
            while (CurrentState == GameState.Joinable)
            {
                // waiting for players
                lock (this) 
                {
                    LobbyUpdate();
                    GameTick++;
                    if (GameTick >= _gameDuration || (GameTick > 5 * _updatesPerSecond && Players.Count == 0))
                        CurrentState = GameState.Ended;
                    foreach (Player p in Players.Values)
                        p.SendGameState();
                }
                SleepBetweenUpdates();

            }
            GameTick = 0;
            while (CurrentState == GameState.Started)
            {
                lock (this)
                {
                    // check if it is time for a new event
                    if (--TicksTillNextEvent < 0)
                        SelectNewEvent();

                    CurrentEvent.EarlyUpdate();
                    Update();
                    CurrentEvent.LateUpdate();
                    GameTick++;
                    // end game
                    if (GameTick > _gameDuration)
                        CurrentState = GameState.Ended;
                    foreach (Player p in Players.Values)
                        p.SendGameState();
                }
                SleepBetweenUpdates();
            }
            EndGame();
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
                if (Entities[i].MyCollider != null)
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
                if (Entities[i].MyCollider == null)
                    continue;
                for (int j = i+1; j < Entities.Count; j++)
                {
                    if (Entities[j].MyCollider == null)
                        continue;
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
            Entities.Clear();
            // reset all players
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Reset();
                for (int j = 0; j < Players[i].GetSpells().Length; j++)
                    Players[i].GetSpells()[j].FullReset();
            }
            GameModifiers.DamageMultiplier = 1;
            SelectNewEvent();
        }

        public void EndGame()
        {
            CurrentState = GameState.Ended;
            foreach (Player p in Players.Values)
                p.SendData("{\"ended\":\"true\"}");
            GameManager.Instance.DeleteGame(Id);
        }

        public Player? GetPlayer(int id, string password)
        {
            if (id < 0 || id >= Players.Count || Players[id].Password != password)
                return null;
            return Players[id];
        }


        private void SleepBetweenUpdates()
        {
            long fullNow = DateTime.Now.Ticks;
            long elapsedTicks = fullNow - _lastUpdateTick;
            long sleepTimeMs = Math.Max(_sleepTimeMs - (elapsedTicks / TimeSpan.TicksPerMillisecond), 1);
            Thread.Sleep((int)sleepTimeMs);
            _lastUpdateTick += _sleepTimeMs * TimeSpan.TicksPerMillisecond;
        }

        public void SelectNewEvent()
        {
            CurrentEvent.End();
            CurrentEvent = NextEvent;
            NextEvent = EventBase.GetRandomEvent(this);
            CurrentEvent.Start();
            TicksTillNextEvent = TicksTillNextEventMax;
        }
    }
}
