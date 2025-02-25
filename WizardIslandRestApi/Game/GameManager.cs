namespace WizardIslandRestApi.Game
{
    public class GameManager
    {
        public static GameManager Instance { get; private set; } = null;
        private int _nextGameId = 0;
        Dictionary<int, Game> _games = new Dictionary<int, Game>();

        public GameManager() 
        {
            if (Instance != null)
                throw (new Exception("A GameManager already exists..."));
            Instance = this;
        }

        public IEnumerable<int> GetAvailableGames()
        {
            return _games.Values.Where(g => g.CanJoin).Select(g => g.Id);
        }
        public Game? GetGame(int gameId)
        {
            if (!_games.ContainsKey(gameId))
                return null;
            return _games[gameId];
        }
        public int CreateNewGame()
        {
            lock (_games)
            {
                int id = _nextGameId++;
                var game = new Game(id);
                _games.Add(id, game);
                Task task = Task.Run(game.GameLoop);
                //task.Start();
                return id;
            }
        }
        public Player JoinGame(int gameId)
        {
            if (!_games.ContainsKey(gameId))
                return null;
            lock (_games[gameId]) 
            {
                return _games[gameId].AddPlayer();
            }
        }
        public void DeleteGame(int gameId)
        {
            lock (_games)
            {
                _games.Remove(gameId);
            }
        }
    }
}
