using System;

namespace ChessWeb.Api.Exceptions
{
    public class UnableToPerformMoveException: Exception
    {
        public override string Message => "Unable to perform the move.";
        public override string ToString() => Message;
    }
}
