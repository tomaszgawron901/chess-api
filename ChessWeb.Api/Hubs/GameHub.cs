using ChessClassLibrary.Models;
using ChessWeb.Api.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessWeb.Api.Extensions;
using ChessClassLibrary.enums;

namespace ChessWeb.Api.Hubs
{
    public class GameHub: Hub
    {
        private readonly GameService gameService;
        private readonly ConnectionToRoomService connectionToRoomService;
        public GameHub(
            GameService gameService,
            ConnectionToRoomService connectionToRoomService)
        {
            this.gameService = gameService;
            this.connectionToRoomService = connectionToRoomService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            
            var user = Context.ConnectionId;
            var roomName = connectionToRoomService.GetRoomId(user);
            var gameRoom = this.gameService.GetGameRoom(roomName);
            connectionToRoomService.RemoveRoomConnection(user);
            if(gameRoom.RemovePlayer(user))
            {
                if (gameRoom.IsRoomEmpty())
                {
                    this.gameService.DeleteGameRoom(roomName);
                }
                else
                {
                    gameRoom.ResetGame();
                    Task.WaitAll(new Task[] {
                        Clients.Group(roomName).SendGameOptions(roomName, gameRoom.gameOptions),
                        Clients.Group(roomName).SendServerMessage(roomName, "player left"),
                    });
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task<string> CreateGameRoom(GameOptions gameOptions)
        {
            var roomCreationResult = this.gameService.CreateNewGameRoom();
            roomCreationResult.gameRoom.StartNewGame(gameOptions);
            return roomCreationResult.key;
        }

        public async Task<GameOptions> JoinGame(string roomName)
        {
            var gameRoom = this.gameService.GetGameRoom(roomName);
            gameRoom.AddMissingPlayer(Context.ConnectionId);
            connectionToRoomService.AddRoomConnection(Context.ConnectionId, roomName);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            Task.WaitAll(new Task[] {
                 Clients.GroupExcept(roomName, Context.ConnectionId).SendGameOptions(roomName, gameRoom.gameOptions),
                 Clients.GroupExcept(roomName, Context.ConnectionId).SendServerMessage(roomName, "new player joined"),
            });
            return gameRoom.gameOptions;
        }

        public Task LeaveGame(string roomName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }


        public async Task PerformMove(string roomName, BoardMove move)
        {
            var gameRoom = this.gameService.GetGameRoom(roomName);
            if (gameRoom.TryPerformMove(Context.ConnectionId, move))
            {
                await Clients.Group(roomName).SendPerformedMove(roomName, move, gameRoom.GetTimer1(), gameRoom.GetTimer2());
            }
        }
    }
}
