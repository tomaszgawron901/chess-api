using ChessWeb.Api.Classes;
using ChessWeb.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ChessWeb.Api.Services
{
    public class GameRoomsService 
    {
        private const int TIME_UNTIL_ROOM_DESTRUCTION = 60000; // 1 min

        private sealed class GameRoomWithCancellation: IDisposable
        {
            public GameRoom GameRoom { get; set; }
            public CancellationTokenSource Cts { get; set; }

            public GameRoomWithCancellation(GameRoom gameRoom, CancellationTokenSource cts)
            {
                GameRoom = gameRoom;
                Cts = cts;
            }

            public void Dispose()
            {
                GameRoom?.Dispose();
                if (Cts != null)
                {
                    if(!Cts.IsCancellationRequested)
                    {
                        Cts.Cancel();
                    }
                    Cts.Dispose();
                }
            }
        }

        private readonly Dictionary<string, GameRoomWithCancellation> GameRooms;
        private readonly IHubContext<GameHub, IGameHubClient> GameHubContext;
        public GameRoomsService(IHubContext<GameHub, IGameHubClient> gameHubContext)
        {
            GameRooms = new Dictionary<string, GameRoomWithCancellation>();
            GameHubContext = gameHubContext;
        }

        /// <summary>
        /// Removes game room associated with given roomKey from service and disposes it.
        /// </summary>
        public void DeleteGameRoom(string roomKey)
        {
            GameRoomWithCancellation roomWithCancellation;
            if (GameRooms.Remove(roomKey, out roomWithCancellation))
            {
                roomWithCancellation.Dispose();
            }
        }

        public bool TryGetGameRoom(string roomKey, out GameRoom gameRoom)
        {
            GameRoomWithCancellation roomWithCancellation;
            var output  = GameRooms.TryGetValue(roomKey, out roomWithCancellation);
            gameRoom = roomWithCancellation.GameRoom;
            return output;
        }

        /// <summary>
        /// Create new game room and add it to dictionary.
        /// </summary>
        /// <returns>created game room</returns>
        public GameRoom CreateNewGameRoom()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;

            string key = CreateKey();

            GameRoom gameRoom = new GameRoom(key, GameHubContext);
            GameRooms.Add(key, new GameRoomWithCancellation(gameRoom, cts));

            _ = Task.Run(() => {
                var canceled = ct.WaitHandle.WaitOne(TIME_UNTIL_ROOM_DESTRUCTION);
                if (!canceled && gameRoom.IsEmpty()) DeleteGameRoom(key);
            }, ct);

            return gameRoom;
        }

        private static string CreateKey()
        {
            using (SHA256 alg = SHA256.Create())
            {
                var msOffset = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var bytes = alg.ComputeHash(BitConverter.GetBytes(msOffset));
                return Convert.ToHexString(bytes);
            }
        }
    }
}
