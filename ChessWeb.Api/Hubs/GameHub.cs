using ChessClassLibrary.Models;
using ChessWeb.Api.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Hubs
{
    public class GameHub: Hub
    {
        private readonly GameService gameService;
        public GameHub(GameService gameService)
        {
            this.gameService = gameService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGame(string roomName)
        {
            var gameRoom = this.gameService.GetGameRoom(roomName);
            gameRoom.AddMissingPlayer(Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            await Clients.GroupExcept(roomName, Context.ConnectionId).SendAsync("GameOptionsChanged", roomName, gameRoom.gameOptions);
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
                await Clients.Group(roomName).SendAsync("PerformMove", roomName, move);
            }
        }
    }
}
