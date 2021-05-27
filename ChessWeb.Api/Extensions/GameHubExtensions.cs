using ChessClassLibrary.enums;
using ChessClassLibrary.Models;
using ChessWeb.Api.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessWeb.Api.Extensions
{
    public static class GameHubExtensions
    {
        public static Task SendGameOptions(this IClientProxy clientProxy, string roomName, GameOptions gameOptions)
        {
            return clientProxy.SendAsync("GameOptionsChanged", roomName, gameOptions);
        }

        public static Task SendPerformedMove(this IClientProxy clientProxy, string roomName, BoardMove move, SharedClock clock1, SharedClock clock2)
        {
            return clientProxy.SendAsync("PerformMove", roomName, move, clock1, clock2);
        }

        public static Task SendServerMessage(this IClientProxy clientProxy, string roomName, string message)
        {
            return clientProxy.SendAsync("ServerMessage", roomName, message);
        }

        public static Task SendUserMessage(this IClientProxy clientProxy, string roomName, string user, string message)
        {
            return clientProxy.SendAsync("UserMessage", roomName, user, message);
        }

        public static Task NotifyGameEnd(this IClientProxy clientProxy, string roomName, PieceColor? winner)
        {
            return clientProxy.SendAsync("GameEnded", roomName, winner);
        }
    }
}
