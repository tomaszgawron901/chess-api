using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Services
{
    public class GameService
    {
        private readonly Dictionary<string, GameManager> GameRooms;
        public GameService()
        {
            this.GameRooms = new Dictionary<string, GameManager>();
        }

        public bool DeleteGameRoom(string roomKey)
        {
            throw new NotImplementedException();
        }

        public GameManager GetGameRoom(string roomKey)
        {
            throw new NotImplementedException();
        }

        public (string key, GameManager gameRoom) CreateNewGameRoom()
        {
            string key = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            GameManager gameRoom = new GameManager();
            GameRooms.Add(key, gameRoom);
            return (key, gameRoom);
        }
    }
}
