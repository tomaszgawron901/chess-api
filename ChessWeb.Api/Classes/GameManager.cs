using ChessClassLibrary.enums;
using ChessClassLibrary.Games;
using ChessClassLibrary.Models;
using ChessWeb.Api.Models;
using System;

namespace ChessWeb.Api.Classes
{
    internal class TimerManager: IDisposable
    {
        public Stopwatch timer;
        public bool moved;
        public double increment;
        public bool isGoing { get; private set; }

        public TimerManager(double time, double increment, Action after)
        {
            this.timer = new Stopwatch(time, (sender, e) => {
                after();
                this.Ended = true;
            });
            this.increment = increment;
            this.moved = false;
            this.Ended = false;
            this.isGoing = false;
        }

        public bool Ended { get; private set; }

        public void Stop()
        {
            this.timer.Stop();
            this.isGoing = false;
        }

        public void HasMoved()
        {
            if(moved)
            {
                this.Stop();
                this.timer.AddTime(this.increment);
            }
            else
            {
                this.moved = true;
            }
        }

        public void TryStart()
        {
            if(moved)
            {
                this.timer.Start();
                this.isGoing = true;
            }
        }

        public void Dispose()
        {
            this.timer.Dispose();
        }
    }

    public class GameManager: IDisposable
    {
        private IGame game;

        private TimerManager whiteTimer;
        private TimerManager blackTimer;

        private Action<PieceColor?> afterWin;
        public GameManager(IGame game, double time, double increment, Action<PieceColor?> afterWin)
        {
            this.game = game;
            whiteTimer = new TimerManager(time, increment, () => this.AfterGameEnd(PieceColor.Black));
            blackTimer = new TimerManager(time, increment, () => this.AfterGameEnd(PieceColor.White));
            this.afterWin = afterWin;
            this.GameState = GameState.NotStarted;
        }

        public GameState GameState { get; private set; }

        private void AfterGameEnd(PieceColor? winner)
        {
            if(GameState != GameState.Ended)
            {
                this.GameState = GameState.Ended;
                this.blackTimer.Stop();
                this.whiteTimer.Stop();
                this.afterWin(winner);
            }
        }

        public SharedClock GetTimer1() => new SharedClock() { Started= whiteTimer.isGoing, Time=whiteTimer.timer.time };
        public SharedClock GetTimer2() => new SharedClock() { Started=blackTimer.isGoing, Time=blackTimer.timer.time };

        public bool TryPerformMove(PieceColor color, BoardMove move)
        {
            if(this.GameState != GameState.Ended && this.game.CurrentPlayerColor == color && this.game.CanPerformMove(move))
            {
                this.game.PerformMove(move);
                if(this.game.GameState == GameState.Ended)
                {
                    this.AfterGameEnd(this.game.GetWinner());
                }
                else
                {
                    if (color == PieceColor.White)
                    {
                        this.whiteTimer.HasMoved();
                        this.blackTimer.TryStart();
                    }
                    else if (color == PieceColor.Black)
                    {

                        this.blackTimer.HasMoved();
                        this.whiteTimer.TryStart();
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
