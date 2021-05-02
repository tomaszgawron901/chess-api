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
    public class GameManager
    {
        public string player1;
        public string player2;

        public IGame game;

        public void StartNewGame(GameOptions gameOptions)
        {
            switch (gameOptions.GameVarient)
            {
                case GameVarient.Standard: this.game = new ClassicGame();
                    break;
                default: this.game = new ClassicGame();
                    break;
            }
        }


    }
}
