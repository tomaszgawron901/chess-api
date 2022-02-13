using System.Collections.Generic;

namespace ChessWeb.Api.Services
{
    public class ConnectionToRoomService
    {
        private readonly Dictionary<string, string> ConnectionToRoomDictionary;
        public ConnectionToRoomService()
        {
            this.ConnectionToRoomDictionary = new Dictionary<string, string>();
        }

        public void AddRoomConnection(string connection, string roomId)
        {
            this.ConnectionToRoomDictionary.Add(connection, roomId);
        }

        public void RemoveRoomConnection(string connection)
        {
            this.ConnectionToRoomDictionary.Remove(connection);
        }

        public string GetRoomId(string connection)
        {
            return ConnectionToRoomDictionary.GetValueOrDefault(connection);
        }
    }
}
