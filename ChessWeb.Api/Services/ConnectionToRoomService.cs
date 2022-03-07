using ChessWeb.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessWeb.Api.Services
{
    public class ConnectionToRoomService
    {
        private readonly Dictionary<string, string> ConnectionToRoomDictionary;
        private readonly IHubContext<GameHub, IGameHubClient> HubContext;
        public ConnectionToRoomService(IHubContext<GameHub, IGameHubClient> gameHubContext)
        {
            ConnectionToRoomDictionary = new Dictionary<string, string>();
            HubContext = gameHubContext;
        }

        public bool IsConnected(string connectionId)
        {
            return ConnectionToRoomDictionary.ContainsKey(connectionId);
        }

        public bool IsConnectedTo(string connectionId, string gameCode)
        {
            string gameCode2;
            return TryGetConnectionRoomKey(connectionId, out gameCode2) && gameCode2 == gameCode;
        }

        public async Task<bool> AddRoomConnection(string connection, string roomKey)
        {
            if(ConnectionToRoomDictionary.TryAdd(connection, roomKey))
            {
                await HubContext.Groups.AddToGroupAsync(connection, roomKey);
                return true;
            }
            return false;
        }

        public Task<bool> TryRemoveConnection(string connectionId, out string roomKey)
        {
            if(ConnectionToRoomDictionary.Remove(connectionId, out roomKey))
            {
                return HubContext.Groups.RemoveFromGroupAsync(connectionId, roomKey).ContinueWith( x => true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> TryRemoveConnection(string connectionId)
        {
            return TryRemoveConnection(connectionId, out _);
        }

        public bool TryGetConnectionRoomKey(string connectionId, out string roomKey)
        {
            return ConnectionToRoomDictionary.TryGetValue(connectionId, out roomKey);
        }
    }
}
