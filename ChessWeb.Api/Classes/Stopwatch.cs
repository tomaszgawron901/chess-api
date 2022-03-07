using System;
using System.Timers;

namespace ChessWeb.Api.Classes
{
    public class Stopwatch: IDisposable
    {
        public double time { get; private set; }
        private DateTime start;
        private Timer timer;

        public Stopwatch(double time, ElapsedEventHandler after)
        {
            this.time = time;
            timer = new Timer();
            timer.Elapsed += after;
            timer.AutoReset = false;
        }

        public void Start()
        {
            timer.Interval = time;
            start = DateTime.Now;
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
            time -= (DateTime.Now - start).TotalMilliseconds;
        }

        public void AddTime(double time)
        {
            this.time += time;
            timer.Interval = this.time;
        }

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
        }

        ~Stopwatch()
        {
            Dispose();
        }
    }
}
