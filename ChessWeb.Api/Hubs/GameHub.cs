using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Hubs
{
    public class GameHub: Hub
    {
        public static Dictionary<string, Object> games;

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }


        public Task CreateGame()
        {
            throw new NotImplementedException();
        }

        public Task StartGame()
        {
            throw new NotImplementedException();
        }

        public Task JoinGame(string roomName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        public Task LeaveGame(string roomName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }
    }
}
