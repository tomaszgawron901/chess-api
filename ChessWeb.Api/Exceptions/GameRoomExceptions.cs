using System;

namespace ChessWeb.Api.Exceptions
{
    public class GameRoomFullException: Exception
    {
        public override string Message => "Game room is full.";
        public override string ToString() => Message;
    }

    public class PlayerNotInTheGameRoomException : Exception
    {
        public override string Message => "Player is not in the room.";
        public override string ToString() => Message;
    }

    public class GameRoomDoesNotExistException : Exception
    {
        public override string Message => "Game room does not exist.";
        public override string ToString() => Message;
    }

    public class UnablToAddToGameRoomException : Exception
    {
        public override string Message => "Unable to add user to game room.";
        public override string ToString() => Message;
    }

    public class AlreadyConnectedToRoomException: Exception
    {
        public override string Message => "ConnectionId is already associated with some game room.";
        public override string ToString() => Message;
    }
}
