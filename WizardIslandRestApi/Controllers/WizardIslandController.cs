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
        //[HttpPost("/Join/{gameId}")]
        //public ActionResult<string> JoinGame(int gameId, [FromBody] PlayerCusomizationData playerCusomizationData)
        //{
        //    try
        //    {

        //        var p = _gameManager.PlayerJoinGame(gameId, playerCusomizationData);
        //        return Ok(new
        //        {
        //            Id = p.Id,
        //            Password = p.Password,
        //            Map = p._game.GameMap,
        //            YourSpells = p.GetSpells().Select(x => x.SpellIndex).ToList(),
        //            GameDuration = WizardIslandRestApi.Game.Game._gameDuration,
        //            EventDuration = p._game.TicksTillNextEventMax,
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

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

        public class PlayerSendDataPacket
        {
            public int PlayerId { get; set; }
            public string Password { get; set; }
            public string ExtraData { get; set; }
        }

        public class SpellData
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
        public enum ActionPacketType
        {
            Move = 0,
            Spell = 1,
        }
    }
}
