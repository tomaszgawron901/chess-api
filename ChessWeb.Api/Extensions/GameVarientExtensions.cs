using ChessClassLib.Enums;
using hessClassLibrary.Logic.Games;

namespace ChessWeb.Api.Extensions
{
    public static class GameVarientExtensions
    {
        public static IGame ConvertToGame(this GameVarient gameVarient)
        {
            switch (gameVarient)
            {
                case GameVarient.Standard:
                    return new ClassicGame();
                case GameVarient.Knightmate:
                    return new KnightmateGame();
                default:
                    return new ClassicGame();
            }
        }
    }
}
