using ChessClassLib.Enums;
using ChessClassLib.Models;
using ChessWeb.Api.Delegates;
using ChessWeb.Api.Models;
using hessClassLibrary.Logic.Games;
using System;

namespace ChessWeb.Api.Classes
{
    

    public class GameManager : IDisposable
    {
        private IGame game;

        private ChessTimerManager whiteTimer;
        private ChessTimerManager blackTimer;

        public WinnerDelegate AfterTimeEnds { get; set; }
        
        public GameManager(IGame game, double time, double increment)
        {
            this.game = game;
            whiteTimer = new ChessTimerManager(time, increment, () => _afterTimeEnds(PieceColor.Black));
            blackTimer = new ChessTimerManager(time, increment, () => _afterTimeEnds(PieceColor.White));
            GameState = GameState.NotStarted;
        }

        public GameState GameState { get; private set; }
        public PieceColor? Winner { get; private set; }

        private void _afterTimeEnds(PieceColor winner)
        {
            if(GameState != GameState.Ended)
            {
                GameState = GameState.Ended;
                Winner = Winner;
                blackTimer.Stop();
                whiteTimer.Stop();
                AfterTimeEnds.Invoke(winner);
            }
        }

        public SharedClock GetTimer1() => new SharedClock() { Started= whiteTimer.isGoing, Time=whiteTimer.timer.time };
        public SharedClock GetTimer2() => new SharedClock() { Started=blackTimer.isGoing, Time=blackTimer.timer.time };

        public bool TryPerformMove(PieceColor color, BoardMove move)
        {
            if(GameState != GameState.Ended && game.CurrentPlayerColor == color && game.TryPerformMove(move))
            {
                GameState = GameState.InProgress;
                if(game.GameState == GameState.Ended)
                {
                    GameState = GameState.Ended;
                    Winner = game.GetWinner();
                }
                else
                {
                    if (color == PieceColor.White)
                    {
                        whiteTimer.HasMoved();
                        blackTimer.TryStart();
                    }
                    else if (color == PieceColor.Black)
                    {

                        blackTimer.HasMoved();
                        whiteTimer.TryStart();
                    }
                }
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (whiteTimer != null) whiteTimer.Dispose();
            if (blackTimer != null) blackTimer.Dispose();
        }
    }
}
