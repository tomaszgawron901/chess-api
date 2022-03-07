using ChessClassLib.Enums;
using ChessClassLib.Models;
using ChessWeb.Api.Exceptions;
using ChessWeb.Api.Extensions;
using ChessWeb.Api.Models;
using System;

namespace ChessWeb.Api.Classes
{
    public class GameRoom: IDisposable
    {
        public GameOptions gameOptions;
        private GameManager gameManager;

        public GameRoom() {}
        public GameManager StartNewGame()
        {
            if (gameOptions != null)
            {
                if (gameManager != null) { gameManager.Dispose(); }
                gameManager = new GameManager(
                    gameOptions.GameVarient.ConvertToGame(),
                    60000D * gameOptions.MinutesPerSide,
                    1000D * gameOptions.IncrementInSeconds
                );
                return gameManager;
            }
            return null;
        }

        public GameManager StartNewGame(GameOptions gameOptions)
        {
            this.gameOptions = gameOptions;
            return StartNewGame();
        }

        public SharedClock GetTimer1() => gameManager.GetTimer1();
        public SharedClock GetTimer2() => gameManager.GetTimer2();

        private bool IsPlayerInRoom(string player)
        {
            return gameOptions?.Player1 == player || gameOptions?.Player2 == player;
        }

        private PieceColor GetPlayerColor(string player)
        {
            if(gameOptions.Player1 == player)
            {
                return PieceColor.White;
            }
            else if(gameOptions.Player2 == player)
            {
                return PieceColor.Black;
            }

            throw new PlayerNotInTheGameRoomException();
        }

        public bool TryAddMissingPlayer(string player)
        {
            if (gameOptions.Side == PieceColor.White)
            {
                return TryAddPlayer1(player) || TryAddPlayer2(player);
            }
            else if(gameOptions.Side == PieceColor.Black)
            {
                return TryAddPlayer2(player) || TryAddPlayer1(player);
            }
            return false;
        }

        private bool TryAddPlayer1(string player)
        {
            if (gameOptions.Player1 == null)
            {
                gameOptions.Player1 = player;
                return true;
            }
            return false;
        }

        private bool TryAddPlayer2(string player)
        {
            if (gameOptions.Player2 == null)
            {
                gameOptions.Player2 = player;
                return true;
            }
            return false;
        }

        public bool RemovePlayer(string player)
        {

            if (gameOptions.Player1 == player)
            {
                gameOptions.Player1 = null;
            }
            else if(gameOptions.Player2 == player)
            {
                gameOptions.Player2 = null;
            }
            else
            {
                return false;
            }
            return true;
        }
        public bool IsEmpty()
        {
            return gameOptions.Player1 == null && gameOptions.Player2 == null;
        }

        public bool IsFull()
        {
            return !(gameOptions.Player1 == null || gameOptions.Player2 == null);
        }

        public GameOptions GetGameOptions()
        {
            return gameOptions;
        }

        public bool TryPerformMove(string player, BoardMove move)
        {
            return IsFull() && IsPlayerInRoom(player) && gameManager.TryPerformMove(GetPlayerColor(player), move);
        }

        public void Dispose()
        {
            if(gameManager != null)
            {
                gameManager.Dispose();
            }
        }
        ~GameRoom()
        {
            Dispose();
        }
    }
}
