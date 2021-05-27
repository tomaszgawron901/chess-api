using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Models
{
    public class SharedClock
    {
        public bool Started { get; set; }
        public double Time { get; set; }
    }
}
