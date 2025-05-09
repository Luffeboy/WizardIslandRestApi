using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WizardIslandRestApi.Game;
using WizardIslandRestApi.Game.Spells;
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
        [HttpPost("/StartGame/{gameId}")]
        public ActionResult<string> StartGame(int gameId, [FromBody] string password)
        {
            var game = _gameManager.GetGame(gameId);
            if (game == null || game.Players[0].Password != password || !game.CanJoin)
                return NotFound();
            lock(game)
            {
                game.StartGame();
            }
            return Ok();
        }
        [HttpPost("/Join/{gameId}")]
        public ActionResult<string> JoinGame(int gameId, [FromBody] PlayerCusomizationData playerCusomizationData)
        {
            var spells = playerCusomizationData.Spells;
            // remove douplicate spells
            for (int i = 0; i < spells.Count; i++)
                for (int j = i+1; j < spells.Count; j++)
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
                    return BadRequest("Those spells don't exist");
            // cast spells indicies to ints
            //int[] spells = new int[spellsAsStrings.Count];
            var game = _gameManager.GetGame(gameId);
            if (game == null)
                return NotFound("Game does not exist");
            // not enough spells
            int allowedSpellCount = game.AllowedSpellCount;
            if (spells.Count < allowedSpellCount)
                return BadRequest("Select  more spells (" + game.AllowedSpellCount + " spells)");
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
                    return BadRequest("Game has allready started!");
                p.UserName = string.IsNullOrEmpty(playerCusomizationData.Name) ? "Nameless" : playerCusomizationData.Name;
                if (p.UserName.Length > 16)
                    p.UserName = p.UserName.Substring(0, 16);
                p.Color = col;
                return Ok(new
                {
                    Id = p.Id,
                    Password = p.Password,
                    Map = game.GameMap,
                    YourSpells = spells,
                    GameDuration = WizardIslandRestApi.Game.Game._gameDuration,
                    EventDuration = game.TicksTillNextEventMax,
                });
            }
        }

        [HttpGet("/AvailableGames")]
        public ActionResult<string> Get()
        {
            var spells = Spell.GetSpells();
            return Ok(new
            {
                Games = _gameManager.GetAvailableGames(),
                SpellTypes = Enum.GetNames(typeof(SpellType)),
                AvailableSpells = spells.Select(spell => new { spell.Name, spell.Type, Cooldown = ((float)spell.CooldownMax / Game.Game._updatesPerSecond) }),
            });
        }
        [HttpGet("/{gameId}")]
        public ActionResult<string> GetGameInfo(int gameId, [FromHeader] int playerId, [FromHeader] string password, [FromHeader] int gameTick)
        {
            var game = _gameManager.GetGame(gameId);
            if (game == null)
                return NotFound("Game does not exist");
            var player = game.GetPlayer(playerId, password);
            if (player == null)
                return NotFound("player does not exist");
            try
            {
                while (true)
                {
                    lock (game)
                    {
                        if (game.CurrentState == GameState.Ended)
                            return StatusCode(418, "The game has ended");
                        if (game.GameTick != gameTick)
                        {
                            return Ok(new 
                            {
                                GameTick = game.GameTick,
                                Players = PlayerMinimum.Copy(game.Players.Values),
                                Entities = game.Entities.Where(e => e.VisableTo == -1 || e.VisableTo == playerId).Select(e => new { e.Pos, e.Size, e.Color, e.EntityId, angle = e.ForwardAngle }).ToArray(),
                                YourSpells = player.GetSpellCooldowns(),
                                Map = game.GameMap,
                                Event = game.CurrentEvent,
                                NextEvent = game.NextEvent,
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

        enum ActionPacketType
        {
            Move = 0,
            Spell = 1,
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
                //Console.WriteLine("Action: " + packetTypeAsString + "\ndata: " + extraData);

                lock (game)
                {
                    switch (packetType)
                    {
                        case (int)ActionPacketType.Move:
                            game.Players[data.PlayerId].TargetPos = JsonSerializer.Deserialize<Vector2>(extraData);
                            break;
                        case (int)ActionPacketType.Spell:
                            SpellData spellData = JsonSerializer.Deserialize<SpellData>(extraData);
                            game.Players[data.PlayerId].CastSpell(spellData.spellIndex, spellData.mousePos);
                            break;
                        //default:
                        //    return BadRequest("Action not available");
                    }
                    return Ok();
                }
            }
            catch (Exception ex) { }
            return NotFound();
        }

        public class PlayerSendDataPacket
        {
            public int PlayerId { get; set; }
            public string Password { get; set; }
            public string ExtraData { get; set; }
        }

        private class SpellData
        {
            public int spellIndex { get; set; }
            public Vector2 mousePos { get; set; }
        }
        public class PlayerCusomizationData
        {
            public List<int> Spells { get; set; }
            public string Name { get; set; }
            public string Color { get; set; }
        }
    }
}
