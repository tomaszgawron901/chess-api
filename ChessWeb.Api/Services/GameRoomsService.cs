using ChessWeb.Api.Classes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ChessWeb.Api.Services
{
    public class GameRoomsService 
    {
        private const int WAIT_UNTIL_ROOM_AUTO_DESTRUCTION = 60000; // 1 min

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
        public GameRoomsService()
        {
            GameRooms = new Dictionary<string, GameRoomWithCancellation>();
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

        public (string key, GameRoom gameRoom) CreateNewGameRoom()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;

            string key = CreateHash();

            GameRoom gameRoom = new GameRoom();
            GameRooms.Add(key, new GameRoomWithCancellation(gameRoom, cts));

            _ = Task.Run(() =>
            {
                var canceled = ct.WaitHandle.WaitOne(WAIT_UNTIL_ROOM_AUTO_DESTRUCTION);
                if (!canceled && gameRoom.IsEmpty())
                {
                    DeleteGameRoom(key);
                }
            }, ct);

            return (key, gameRoom);
        }

        private static string CreateHash()
        {
            using (SHA256 alg = SHA256.Create())
            {
                var bytes = alg.ComputeHash(BitConverter.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds()));
                return Convert.ToHexString(bytes);
            }
        }
    }
}
