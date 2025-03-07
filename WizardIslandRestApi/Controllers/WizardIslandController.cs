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
        public ActionResult<string> JoinGame(int gameId, [FromBody] List<int> spells)
        {
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
            lock (game)
            {
                Player? p = game.AddPlayer(spells.ToArray());
                if (p == null)
                    return BadRequest("Game has allready started!");
                return Ok(new { 
                    Id = p.Id, 
                    Password = p.Password,
                    Map = game.GameMap,
                    YourSpells = spells,
                });
            }
        }

        [HttpGet("/AvailableGames")]
        public ActionResult<string> Get()
        {
            return Ok(new { Games = _gameManager.GetAvailableGames(), AvailableSpells = Spell.GetSpells().Select(spell => spell.Name) });
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
                                Players = PlayerMinimum.Copy(game.Players.Values.Where(p => !p.IsDead)),
                                Entities = game.Entities.Select(e => new { e.Pos, e.Size, e.Color }),
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
    }
}
