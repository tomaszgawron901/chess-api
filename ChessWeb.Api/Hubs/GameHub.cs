using ChessWeb.Api.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using ChessWeb.Api.Models;
using ChessClassLib.Models;
using ChessClassLib.Enums;
using ChessWeb.Api.Classes;
using ChessWeb.Api.Exceptions;

namespace ChessWeb.Api.Hubs
{
    public interface IGameHubClient
    {
        Task GameOptionsChanged(string roomName, GameOptions gameOptions);
        Task PerformMove(string roomName, BoardMove move, SharedClock clock1, SharedClock clock2);
        Task GameEnded(string roomName, PieceColor? winner);
        Task PlayerLeft(string roomName, string player);
        Task PlayerJoined(string roomName, string player);
    }


    [SignalRHub(path: "/gamehub")]
    public class GameHub: Hub<IGameHubClient>
    {
        private readonly GameRoomsService gameRoomsService;
        private readonly ConnectionToRoomService connectionToRoomService;
        public GameHub(
            GameRoomsService gameRoomsService,
            ConnectionToRoomService connectionToRoomService)
        {
            this.gameRoomsService = gameRoomsService;
            this.connectionToRoomService = connectionToRoomService;
        }

        [SignalRHidden]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            string roomName;
            GameRoom room;
            if (
                await connectionToRoomService.TryRemoveConnection(connectionId, out roomName) &&
                gameRoomsService.TryGetGameRoom(roomName, out room) &&
                room.RemovePlayer(connectionId)
            ) {
                if (room.IsEmpty())
                {
                    gameRoomsService.DeleteGameRoom(roomName);
                }
                else
                {
                    await Task.WhenAll(
                        Clients.Group(roomName).GameOptionsChanged(roomName, room.gameOptions),
                        Clients.Group(roomName).PlayerLeft(roomName, connectionId)
                    );
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        [SignalRMethod(
            summary: "Creates a game room with given options and associates creator to the room. Returns the unique game key",
            autoDiscover: AutoDiscover.Params
        )]
        public async Task<string> CreateGameRoom(GameOptions gameOptions)
        {
            var (key, gameRoom) = gameRoomsService.CreateNewGameRoom();
            gameRoom.gameOptions = gameOptions;

            return key;
        }

        [SignalRMethod(
            summary: "Associates sender to game room with given name. Returns game options of the game room.",
            autoDiscover: AutoDiscover.Params
        )]
        public Task<GameOptions> JoinGame(string roomName)
        {
            string connectionId = Context.ConnectionId;
            if (connectionToRoomService.IsConnected(connectionId))
            {
                throw new AlreadyConnectedToRoomException();
            }

            GameRoom gameRoom;
            if(gameRoomsService.TryGetGameRoom(roomName, out gameRoom))
            {
                if (gameRoom.TryAddMissingPlayer(connectionId))
                {
                    if(gameRoom.IsFull())
                    {
                        gameRoom.StartNewGame().AfterTimeEnds = async (winner) => await Clients.Group(roomName).GameEnded(roomName, winner);
                    }

                    return Task.WhenAll(
                        Clients.Group(roomName).GameOptionsChanged(roomName, gameRoom.gameOptions),
                        Clients.Group(roomName).PlayerJoined(roomName, Context.ConnectionId)
                        )
                        .ContinueWith(_ => connectionToRoomService.AddRoomConnection(connectionId, roomName)).Unwrap()
                        .ContinueWith(_ => gameRoom.gameOptions);
                }
                throw new UnablToAddToGameRoomException();
            }
            throw new GameRoomDoesNotExistException();
        }

        [SignalRMethod(
            summary: "Removes sender from the game room with given game room name.",
            autoDiscover: AutoDiscover.Params
        )]
        public async Task LeaveGame(string roomName)
        {
            var connectionId = Context.ConnectionId;
            GameRoom room;
            if (
                connectionToRoomService.IsConnectedTo(connectionId, roomName) &&
                await connectionToRoomService.TryRemoveConnection(connectionId) &&
                gameRoomsService.TryGetGameRoom(roomName, out room) &&
                room.RemovePlayer(connectionId)
            )
            {
                if (room.IsEmpty())
                {
                    gameRoomsService.DeleteGameRoom(roomName);
                }
                else
                {
                    await Task.WhenAll(
                        Clients.Group(roomName).GameOptionsChanged(roomName, room.gameOptions),
                        Clients.Group(roomName).PlayerLeft(roomName, connectionId)
                    );
                }
            }
        }


        [SignalRMethod(
            summary: "If possible performs given move on board in the game room with given game room name.",
            autoDiscover: AutoDiscover.Params
        )]
        public async Task PerformMove(string roomKey, BoardMove move)
        {
            GameRoom gameRoom;
            if(gameRoomsService.TryGetGameRoom(roomKey, out gameRoom))
            {
                if (gameRoom.TryPerformMove(Context.ConnectionId, move))
                {
                    await Clients.Group(roomKey).PerformMove(roomKey, move, gameRoom.GetTimer1(), gameRoom.GetTimer2());
                    return;
                }
                throw new UnableToPerformMoveException();
            }
            throw new GameRoomDoesNotExistException();

        }
    }
}
