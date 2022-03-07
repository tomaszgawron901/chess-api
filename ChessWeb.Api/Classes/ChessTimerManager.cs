using System;

namespace ChessWeb.Api.Classes
{
    public class ChessTimerManager : IDisposable
    {
        public Stopwatch timer;
        public bool moved;
        public double increment;
        public bool isGoing { get; private set; }

        public ChessTimerManager(double time, double increment, Action after)
        {
            timer = new Stopwatch(time, (sender, e) => {
                after();
                Ended = true;
            });
            this.increment = increment;
            moved = false;
            Ended = false;
            isGoing = false;
        }

        public bool Ended { get; private set; }

        public void Stop()
        {
            timer.Stop();
            isGoing = false;
        }

        public void HasMoved()
        {
            if (moved)
            {
                Stop();
                timer.AddTime(increment);
            }
            else
            {
                moved = true;
            }
        }

        public void TryStart()
        {
            if (moved)
            {
                timer.Start();
                isGoing = true;
            }
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
