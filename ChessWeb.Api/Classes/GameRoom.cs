using ChessClassLib.Enums;
using ChessClassLib.Models;
using ChessWeb.Api.Exceptions;
using ChessWeb.Api.Extensions;
using ChessWeb.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ChessWeb.Api.Classes
{
    public class GameRoom: IDisposable
    {
        public GameOptions GameOptions { get; set; }
        private GameManager GameManager { get; set; }
        public string RoomKey { get; }
        private IHubContext<GameHub, IGameHubClient> HubContext { get; }

        public GameRoom(string roomKey, IHubContext<GameHub, IGameHubClient> hubContext) {
            RoomKey = roomKey;
            HubContext = hubContext;
        }

        public void StartNewGame()
        {
            if (GameOptions != null)
            {
                if (GameManager != null) { GameManager.Dispose(); }
                GameManager = new GameManager(
                    GameOptions.GameVarient.ConvertToGame(),
                    60000D * GameOptions.MinutesPerSide,
                    1000D * GameOptions.IncrementInSeconds
                );
                GameManager.AfterTimeEnds += NotifyGameEnded;
            }
        }

        public void StartNewGame(GameOptions gameOptions)
        {
            this.GameOptions = gameOptions;
            StartNewGame();
        }

        private async void NotifyGameEnded(PieceColor? winner)
        {
            await HubContext.Clients.Group(RoomKey).GameEnded(RoomKey, winner);
        }

        private bool IsPlayerInRoom(string player)
        {
            return GameOptions?.Player1 == player || GameOptions?.Player2 == player;
        }

        private PieceColor GetPlayerColor(string player)
        {
            if(GameOptions.Player1 == player)
            {
                return PieceColor.White;
            }
            else if(GameOptions.Player2 == player)
            {
                return PieceColor.Black;
            }

            throw new PlayerNotInTheGameRoomException();
        }

        public async Task<bool> TryAddMissingPlayer(string player)
        {
            if (player == null) return false;

            if (GameOptions.Side == PieceColor.White)
            {
                if(!TryAddPlayer1(player) && !TryAddPlayer2(player))
                {
                    return false;
                }
            }
            else if(GameOptions.Side == PieceColor.Black)
            {
                if (!TryAddPlayer2(player) && !TryAddPlayer1(player))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            await HubContext.Clients.Group(RoomKey).GameOptionsChanged(RoomKey, GameOptions);
            await HubContext.Groups.AddToGroupAsync(player, RoomKey);
            await HubContext.Clients.GroupExcept(RoomKey, player).PlayerJoined(RoomKey, player);
            return true;
        }

        private bool TryAddPlayer1(string player)
        {
            if (GameOptions.Player1 == null)
            {
                GameOptions.Player1 = player;
                return true;
            }
            return false;
        }

        private bool TryAddPlayer2(string player)
        {
            if (GameOptions.Player2 == null)
            {
                GameOptions.Player2 = player;
                return true;
            }
            return false;
        }

        public async Task<bool> TryRemovePlayer(string player)
        {
            if (GameOptions.Player1 == player)
            {
                GameOptions.Player1 = null;
            }
            else if(GameOptions.Player2 == player)
            {
                GameOptions.Player2 = null;
            }
            else
            {
                return false;
            }
            await HubContext.Groups.RemoveFromGroupAsync(player, RoomKey);
            await HubContext.Clients.Group(RoomKey).PlayerLeft(RoomKey, player);
            return true;
        }
        public bool IsEmpty()
        {
            return GameOptions.Player1 == null && GameOptions.Player2 == null;
        }

        public bool IsFull()
        {
            return !(GameOptions.Player1 == null || GameOptions.Player2 == null);
        }

        public async Task<bool> TryPerformMove(string player, BoardMove move)
        {
            if(IsFull() && IsPlayerInRoom(player) && GameManager.TryPerformMove(GetPlayerColor(player), move))
            {
                await HubContext.Clients.Group(RoomKey).PerformMove(RoomKey, move, GameManager.GetTimer1(), GameManager.GetTimer2());
                if (GameManager.GameState == GameState.Ended)
                {
                    await HubContext.Clients.Group(RoomKey).GameEnded(RoomKey, GameManager.Winner);
                }
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if(GameManager != null)
            {
                GameManager.Dispose();
            }
        }
        ~GameRoom()
        {
            Dispose();
        }
    }
}
