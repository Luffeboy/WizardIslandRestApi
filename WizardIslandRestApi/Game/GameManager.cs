using System.Net.WebSockets;
using System.Text.Json;
using WizardIslandRestApi.Game.Spells;
using static WizardIslandRestApi.Controllers.WizardIslandController;

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
            //Console.WriteLine("All games: " + _games.Values.Count + "Available games: " + _games.Values.Where(g => g.CanJoin).Select(g => g.Id).Count());
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
        //public Player JoinGame(int gameId)
        //{
        //    if (!_games.ContainsKey(gameId))
        //        return null;
        //    lock (_games[gameId]) 
        //    {
        //        return _games[gameId].AddPlayer();
        //    }
        //}
        public void DeleteGame(int gameId)
        {
            lock (_games)
            {
                _games.Remove(gameId);
            }
        }

        public async Task JoinAndPlayGame(int id, WebSocket socket)
        {
            try
            {
                byte[] buffer = new byte[1024];
                await socket.ReceiveAsync(buffer, CancellationToken.None);
                string recievedText = System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                PlayerCusomizationData? playerCusomizationData = JsonSerializer.Deserialize<PlayerCusomizationData>(recievedText);
                if (playerCusomizationData is null)
                    throw new Exception("Invalid player customization data");
                var player = PlayerJoinGame(id, playerCusomizationData);
                // send starting data
                player.SetWebSocket(socket);
                player.SendData(new
                {
                    Id = player.Id,
                    Password = player.Password,
                    Map = player._game.GameMap,
                    YourSpells = player.GetSpells().Select(x => x.SpellIndex).ToList(),
                    GameDuration = WizardIslandRestApi.Game.Game._gameDuration,
                    EventDuration = player._game.TicksTillNextEventMax,
                });
                // read data, until game closes
                await player.readSocketDataAsync();
            }
            catch (Exception ex)
            {
                await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Could not join game: " + ex.Message, CancellationToken.None);
                return;
            }
        }
        public Player PlayerJoinGame(int gameId, PlayerCusomizationData playerCusomizationData)
        {
            var spells = playerCusomizationData.Spells;
            // remove douplicate spells
            for (int i = 0; i < spells.Count; i++)
                for (int j = i + 1; j < spells.Count; j++)
                {
                    if (spells[i] == spells[j])
                    {
                        spells.RemoveAt(j);
                        j--;
                    }
                }
            // check none of the spells are above the amount of spells we have
            int maxSpells = Spell.GetSpells().Length;
            for (int i = 0; i < spells.Count; i++)
                if (spells[i] < 0 || spells[i] >= maxSpells)
                    throw new Exception("Those spells don't exist");
            // cast spells indicies to ints
            var game = GetGame(gameId);
            if (game == null)
                throw new Exception("Game does not exist");
            // not enough spells
            int allowedSpellCount = game.AllowedSpellCount;
            if (spells.Count < allowedSpellCount)
                throw new Exception("Select  more spells (" + game.AllowedSpellCount + " spells)");
            // too many spells, we just cut a few off
            if (spells.Count > allowedSpellCount)
            {
                int[] temp = new int[allowedSpellCount];
                for (int i = 0; i < allowedSpellCount; i++)
                    temp[i] = spells[i];
                spells = temp.ToList();
            }
            // check the color
            string col = playerCusomizationData.Color;

            if (!string.IsNullOrEmpty(col))
            {
                col = col.Replace(" ", "");
                var colValues = col.Split(',');
                if (colValues.Length == 3 && int.TryParse(colValues[0], out int value) && int.TryParse(colValues[1], out int value1) && int.TryParse(colValues[2], out int value2))
                {
                    if (value < 0 || value > 255 || value1 < 0 || value1 > 255 || value2 < 0 || value2 > 255)
                        col = "255,0,0";
                    else col = value + "," + value1 + "," + value2;
                }
                else col = "255,0,0";
            }
            else col = "255,0,0";
            lock (game)
            {
                Player? p = game.AddPlayer(spells.ToArray());
                if (p == null)
                    throw new Exception("Game has already started!");
                p.UserName = string.IsNullOrEmpty(playerCusomizationData.Name) ? "Nameless" : playerCusomizationData.Name;
                if (p.UserName.Length > 16)
                    p.UserName = p.UserName.Substring(0, 16);
                p.Color = col;
                return p;
            }

        }
    }
}
