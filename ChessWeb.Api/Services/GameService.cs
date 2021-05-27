using ChessClassLibrary.enums;
using ChessClassLibrary.Games;
using ChessClassLibrary.Models;
using ChessWeb.Api.Classes;
using ChessWeb.Api.Extensions;
using ChessWeb.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Services
{
    public class GameService 
    {
        private readonly Dictionary<string, GameRoom> GameRooms;
        private readonly IHubContext<GameHub> gameHubContext;
        public GameService(IHubContext<GameHub> gameHubContext)
        {
            this.GameRooms = new Dictionary<string, GameRoom>();

            this.gameHubContext = gameHubContext;
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
            GameRoom gameRoom = new GameRoom( async (winner) => {
                await NotifyGameEnd(key, winner);
            });
            GameRooms.Add(key, gameRoom);
            return (key, gameRoom);
        }

        private async Task NotifyGameEnd(string key, PieceColor? winner)
        {
            await this.gameHubContext.Clients.Group(key).NotifyGameEnd(key, winner);
        }

        public GameOptions GetGameOptionsByKey(string gameKey)
        {
            return this.GameRooms.GetValueOrDefault(gameKey).gameOptions;
        }
    }
}
