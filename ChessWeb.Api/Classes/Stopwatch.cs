using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            this.timer = new Timer();
            this.timer.Elapsed += after;
            this.timer.AutoReset = false;
        }

        public void Start()
        {
            this.timer.Interval = time;
            Console.WriteLine(time);
            this.start = DateTime.Now;
            this.timer.Start();
        }

        public void Stop()
        {
            this.timer.Stop();
            this.time -= (DateTime.Now - start).TotalMilliseconds;
        }

        public void AddTime(double time)
        {
            this.time += time;
            this.timer.Interval = this.time;
        }

        public void Dispose()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
            }
        }

        ~Stopwatch()
        {
            this.Dispose();
        }
    }
}
