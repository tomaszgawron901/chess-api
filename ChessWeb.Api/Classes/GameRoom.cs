using ChessClassLibrary.enums;
using ChessClassLibrary.Games;
using ChessClassLibrary.Games.ClassicGame;
using ChessClassLibrary.Models;
using ChessWeb.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Classes
{
    public class GameRoom: IDisposable
    {
        public GameOptions gameOptions;
        private GameManager gameManager;

        private Action<PieceColor?> afterWin;

        public GameRoom(Action<PieceColor?> afterWin)
        {
            this.afterWin = afterWin;
        }

        public void StartNewGame(GameOptions gameOptions)
        {
            this.gameOptions = gameOptions;
            this.gameManager = new GameManager(
                this.ConverGameVarientToGame(this.gameOptions.GameVarient), 
                60000D * this.gameOptions.MinutesPerSide, 
                1000D * this.gameOptions.IncrementInSeconds,
                this.afterWin
            );
        }

        public SharedClock GetTimer1() => gameManager.GetTimer1();
        public SharedClock GetTimer2() => gameManager.GetTimer2();

        private IGame ConverGameVarientToGame(GameVarient gameVarient)
        {
            switch (gameVarient)
            {
                case GameVarient.Standard:
                    return new ClassicGame();
                case GameVarient.Knightmate:
                    return new KnightmateGame();
                default:
                    return new ClassicGame();
            }
        }

        public void ResetGame()
        {
            if (this.gameOptions != null)
            {
                this.gameManager.Dispose();
                this.StartNewGame(this.gameOptions);
            }
        }

        private bool IsPlayerInRoom(string player)
        {
            return this.gameOptions?.Player1 == player || this.gameOptions?.Player2 == player;
        }

        private PieceColor ConvertPlayerToColor(string player)
        {
            if(this.gameOptions.Player1 == player)
            {
                return PieceColor.White;
            }
            else if(this.gameOptions.Player2 == player)
            {
                return PieceColor.Black;
            }

            throw new InvalidCastException();
        }

        public void AddMissingPlayer(string player)
        {
            if (this.gameOptions.Side == PieceColor.White)
            {
                if (!(this.TryAddPlayer1(player) || this.TryAddPlayer2(player)))
                {
                    throw new Exception("Room is full.");
                }
            }
            else if(this.gameOptions.Side == PieceColor.Black)
            {
                if (!(this.TryAddPlayer2(player) || this.TryAddPlayer1(player)))
                {
                    throw new Exception("Room is full.");
                }
            }
        }

        private bool TryAddPlayer1(string player)
        {
            if (this.gameOptions.Player1 == null)
            {
                this.gameOptions.Player1 = player;
                return true;
            }
            return false;
        }

        private bool TryAddPlayer2(string player)
        {
            if (this.gameOptions.Player2 == null)
            {
                this.gameOptions.Player2 = player;
                return true;
            }
            return false;
        }


        public bool RemovePlayer(string player)
        {

            if (this.gameOptions.Player1 == player)
            {
                this.gameOptions.Player1 = null;
            }
            else if(this.gameOptions.Player2 == player)
            {
                this.gameOptions.Player2 = null;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool IsRoomEmpty()
        {
            return this.gameOptions.Player1 == null && this.gameOptions.Player2 == null;
        }

        public bool IsRoomFull()
        {
            return !(this.gameOptions.Player1 == null || this.gameOptions.Player2 == null);
        }

        public GameOptions GetGameOptions()
        {
            return this.gameOptions;
        }

        public bool TryPerformMove(string player, BoardMove move)
        {
            return IsRoomFull() && this.IsPlayerInRoom(player) && this.gameManager.TryPerformMove(this.ConvertPlayerToColor(player), move);
        }

        public void Dispose()
        {
            if(this.gameManager != null)
            {
                this.gameManager.Dispose();
            }
        }

        ~GameRoom()
        {
            this.Dispose();
        }
    }
}
