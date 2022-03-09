using System.Collections.Generic;

namespace ChessWeb.Api.Services
{
    public class ConnectionToRoomService
    {
        private readonly Dictionary<string, string> ConnectionToRoomDictionary;
        public ConnectionToRoomService()
        {
            ConnectionToRoomDictionary = new Dictionary<string, string>();
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

        public bool AddRoomConnection(string connection, string roomKey)
        {
            if(ConnectionToRoomDictionary.TryAdd(connection, roomKey))
            {
                return true;
            }
            return false;
        }

        public bool TryRemoveConnection(string connectionId, out string roomKey)
        {
            if(ConnectionToRoomDictionary.Remove(connectionId, out roomKey))
            {
                return true;
            }
            return false;
        }

        public bool TryRemoveConnection(string connectionId)
        {
            return TryRemoveConnection(connectionId, out _);
        }

        public bool TryGetConnectionRoomKey(string connectionId, out string roomKey)
        {
            return ConnectionToRoomDictionary.TryGetValue(connectionId, out roomKey);
        }
    }
}
