using ChessClassLibrary.Models;
using ChessWeb.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly GameService gameService;
        public GameController(GameService gameService)
        {
            this.gameService = gameService;
        }

        [HttpPost]
        public ActionResult<string> CreateGameRoom(GameOptions gameOptions)
        {
            try
            {
                var roomCreationResult = this.gameService.CreateNewGameRoom();
                roomCreationResult.gameRoom.StartNewGame(gameOptions);
                return Ok(roomCreationResult.key);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
