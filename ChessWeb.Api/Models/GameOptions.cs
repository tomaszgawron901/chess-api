using ChessClassLib.Enums;

namespace ChessWeb.Api.Models
{
    public class GameOptions
    {
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public GameVarient GameVarient { get; set; }
        public int MinutesPerSide { get; set; }
        public int IncrementInSeconds { get; set; }
        public PieceColor Side { get; set; }
    }
}
