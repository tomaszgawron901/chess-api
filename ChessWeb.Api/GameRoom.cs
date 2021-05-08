using ChessClassLibrary.enums;
using ChessClassLibrary.Games;
using ChessClassLibrary.Games.ClassicGame;
using ChessClassLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api
{
    public class GameRoom
    {
        public IGame game;
        public GameOptions gameOptions;

        public void StartNewGame(GameOptions gameOptions)
        {
            this.gameOptions = gameOptions;
            switch (gameOptions.GameVarient)
            {
                case GameVarient.Standard: this.game = new ClassicGame();
                    break;
                default: this.game = new ClassicGame();
                    break;
            }
        }

        private bool IsPlayerTurn(string player)
        {
            return (this.game.CurrentPlayerColor == PieceColor.White && this.gameOptions.Player1 == player) ||
                (this.game.CurrentPlayerColor == PieceColor.Black && this.gameOptions.Player2 == player);
        }

        public void AddMissingPlayer(string player)
        {
            if (this.gameOptions.Player1 == null)
            {
                this.gameOptions.Player1 = player;
            }
            else if(this.gameOptions.Player2 == null)
            {
                this.gameOptions.Player2 = player;
            }
            else
            {
                throw new Exception("Room is full.");
            }
        }

        public IGame GetGame()
        {
            return this.game;
        }

        public GameOptions GetGameOptions()
        {
            return this.gameOptions;
        }

        public bool TryPerformMove(string player, BoardMove move)
        {
            if (this.IsPlayerTurn(player))
            {
                try
                {
                    this.game.TryPerformMove(move);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


    }
}
