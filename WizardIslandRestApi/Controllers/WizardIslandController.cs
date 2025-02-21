using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WizardIslandRestApi.Game;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace WizardIslandRestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WizardIslandController : ControllerBase
    {
        private readonly GameManager _gameManager;

        public WizardIslandController(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        [HttpPost("/CreateGame")]
        public ActionResult<string> CreateGame()
        {
            int id = _gameManager.CreateNewGame();
            return Created("/" + id, "Game created");
        }
        [HttpPost("/Join/{gameId}")]
        public ActionResult<string> JoinGame(int gameId)
        {
            var game = _gameManager.GetGame(gameId);
            if (game == null)
                return NotFound("Game does not exist");
            lock(game)
            {
                Player p = game.AddPlayer();
                return Ok(new { 
                    Id = p.Id, 
                    Password = p.Password,
                    Map = game.GameMap,
                });
            }
        }

        [HttpGet("/AvailableGames")]
        public ActionResult<IEnumerable<int>> Get()
        {
            return Ok(_gameManager.GetAvailableGames());
        }
        [HttpGet("/{gameId}")]
        public ActionResult<string> GetGameInfo(int gameId, [FromHeader] int playerId, [FromHeader] string password, [FromHeader] int gameTick)
        {
            var game = _gameManager.GetGame(gameId);
            if (game == null)
                return NotFound("Game does not exist");
            try
            {
                while (true)
                {
                    lock (game)
                    {
                        if (game.GameTick != gameTick)
                        {
                            return Ok(new 
                            {
                                GameTick = game.GameTick,
                                players = PlayerMinimum.Copy(game.Players.Values)
                            });
                        }
                    }
                    // still on the same game tick
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex) { }
            return NotFound("Game does not exist");
        }

        [HttpPost("/{gameId}")]
        public ActionResult<string> DoAction(int gameId, PlayerSendDataPacket data)
        {
            try
            {
                var game = _gameManager.GetGame(gameId);
                if (game == null)
                    return NotFound();
                // get action type
                string packetTypeAsString = "";
                int index;
                for (index = 0; index < data.ExtraData.Length; index++)
                {
                    if (data.ExtraData[index] < '0' || data.ExtraData[index] > '9')
                        break;
                    packetTypeAsString += data.ExtraData[index];
                }
                if (index == 0 || index == data.ExtraData.Length)
                    return BadRequest("Unable to parse action data");
                int packetType = int.Parse(packetTypeAsString);
                string extraData = data.ExtraData.Substring(index);
                // for packet inspection
                // Console.WriteLine("Action: " + packetTypeAsString + "\ndata: " + extraData);

                lock (game)
                {
                    game.Players[data.PlayerId].TargetPos = JsonSerializer.Deserialize<Vector2>(extraData);
                    return Ok();
                }
            }
            catch (Exception ex) { }
            return NotFound();
        }

        public class PlayerSendDataPacket()
        {
            public int PlayerId { get; set; }
            public string Password { get; set; }
            public string ExtraData { get; set; }
        }
    }
}
