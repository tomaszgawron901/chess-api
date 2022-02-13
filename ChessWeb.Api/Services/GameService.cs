using ChessClassLibrary.enums;
using ChessClassLibrary.Models;
using ChessWeb.Api.Classes;
using ChessWeb.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessWeb.Api.Services
{
    public class GameService 
    {
        private readonly Dictionary<string, GameRoom> GameRooms;
        private readonly IHubContext<GameHub, IGameHubClient> gameHubContext;
        public GameService(IHubContext<GameHub, IGameHubClient> gameHubContext)
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
            await gameHubContext.Clients.Group(key).GameEnded(key, winner);
        }

        public GameOptions GetGameOptionsByKey(string gameKey)
        {
            return this.GameRooms.GetValueOrDefault(gameKey).gameOptions;
        }
    }
}
