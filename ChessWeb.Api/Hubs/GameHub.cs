using ChessClassLibrary.Models;
using ChessWeb.Api.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
using ChessWeb.Api.Models;
using ChessClassLibrary.enums;

namespace ChessWeb.Api.Hubs
{
    public interface IGameHubClient
    {
        Task GameOptionsChanged(string roomName, GameOptions gameOptions);
        Task PerformMove(string roomName, BoardMove move, SharedClock clock1, SharedClock clock2);
        Task GameEnded(string roomName, PieceColor? winner);
        Task ServerMessage(string roomName, string message);
    }


    [SignalRHub(path: "/gamehub")]
    public class GameHub: Hub<IGameHubClient>
    {
        private readonly GameService gameService;
        private readonly ConnectionToRoomService connectionToRoomService;
        public GameHub(
            GameService gameService,
            ConnectionToRoomService connectionToRoomService)
        {
            this.gameService = gameService;
            this.connectionToRoomService = connectionToRoomService;
        }

        [SignalRHidden]
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        [SignalRHidden]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            
            var user = Context.ConnectionId;
            var roomName = connectionToRoomService.GetRoomId(user);
            var gameRoom = this.gameService.GetGameRoom(roomName);
            connectionToRoomService.RemoveRoomConnection(user);
            if(gameRoom.RemovePlayer(user))
            {
                if (gameRoom.IsRoomEmpty())
                {
                    this.gameService.DeleteGameRoom(roomName);
                }
                else
                {
                    gameRoom.ResetGame();
                    Task.WaitAll(new Task[] {
                        Clients.Group(roomName).GameOptionsChanged(roomName, gameRoom.gameOptions),
                        Clients.Group(roomName).ServerMessage(roomName, "player left"),
                    });
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
            var roomCreationResult = this.gameService.CreateNewGameRoom();
            roomCreationResult.gameRoom.StartNewGame(gameOptions);
            return roomCreationResult.key;
        }


        [SignalRMethod(
            summary: "Associates sender to game room with given name. Returns game options of the game room.",
            autoDiscover: AutoDiscover.Params
        )]
        public async Task<GameOptions> JoinGame(string roomName)
        {
            var gameRoom = this.gameService.GetGameRoom(roomName);
            gameRoom.AddMissingPlayer(Context.ConnectionId);
            connectionToRoomService.AddRoomConnection(Context.ConnectionId, roomName);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            Task.WaitAll(new Task[] {
                 Clients.GroupExcept(roomName, Context.ConnectionId).GameOptionsChanged(roomName, gameRoom.gameOptions),
                 Clients.GroupExcept(roomName, Context.ConnectionId).ServerMessage(roomName, "new player joined"),
            });
            return gameRoom.gameOptions;
        }

        [SignalRMethod(
            summary: "Removes sender from the game room with given game room name.",
            autoDiscover: AutoDiscover.Params
        )]
        public async Task LeaveGame(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }


        [SignalRMethod(
            summary: "If possible performs given move on board in the game room with given game room name.",
            autoDiscover: AutoDiscover.Params
        )]
        public async Task PerformMove(string roomName, BoardMove move)
        {
            var gameRoom = this.gameService.GetGameRoom(roomName);
            if (gameRoom.TryPerformMove(Context.ConnectionId, move))
            {
                await Clients.Group(roomName).PerformMove(roomName, move, gameRoom.GetTimer1(), gameRoom.GetTimer2());
            }
        }
    }
}
