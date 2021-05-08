using ChessClassLibrary.Models;
using ChessWeb.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly GameService gameService;
        public GameController(GameService gameService)
        {
            this.gameService = gameService;
        }

        [HttpPost]
        public ActionResult CreateGameRoom([FromBody]GameOptions gameOptions)
        {
            try
            {
                var roomCreationResult = this.gameService.CreateNewGameRoom();
                roomCreationResult.gameRoom.StartNewGame(gameOptions);
                return Created(roomCreationResult.key, gameOptions);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public ActionResult GetGameOptionsByKey(string gameKey)
        {
            try
            {
                return Ok(this.gameService.GetGameOptionsByKey(gameKey));
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
