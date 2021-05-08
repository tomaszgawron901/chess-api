using ChessClassLibrary.Games;
using ChessClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Services
{
    public class GameService 
    {
        private readonly Dictionary<string, GameRoom> GameRooms;
        public GameService()
        {
            this.GameRooms = new Dictionary<string, GameRoom>();
        }

        public void DeleteGameRoom(string roomKey)
        {
            this.GameRooms.Remove(roomKey);
        }

        public GameRoom GetGameRoom(string roomKey)
        {
            if (GameRooms.ContainsKey(roomKey))
            {
                return GameRooms.GetValueOrDefault(roomKey);
            }
            else
            {
                throw new KeyNotFoundException("Room does not exist.");
            }
        }

        public (string key, GameRoom gameRoom) CreateNewGameRoom()
        {
            string key = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            GameRoom gameRoom = new GameRoom();
            GameRooms.Add(key, gameRoom);
            return (key, gameRoom);
        }

        public GameOptions GetGameOptionsByKey(string gameKey)
        {
            return this.GameRooms.GetValueOrDefault(gameKey).gameOptions;
        }

        public IGame GetGameByKey(string gameKey)
        {
            return this.GameRooms.GetValueOrDefault(gameKey).game;
        }
    }
}
