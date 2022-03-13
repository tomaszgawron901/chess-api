using ChessClassLib.Enums;

namespace ChessWeb.Api.Models
{
    public class CreateGameOptions
    {
        public GameVarient GameVarient { get; set; }
        public int MinutesPerSide { get; set; }
        public int IncrementInSeconds { get; set; }
        public PieceColor Side { get; set; }
    }
}
