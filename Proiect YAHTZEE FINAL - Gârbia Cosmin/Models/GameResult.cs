using System;

namespace YahtzeeProject.Services
{
    public class GameResult
    {
        public string UserName { get; set; }
        public int UserScore { get; set; }
        public int AIScore { get; set; }
        public string AIType { get; set; } // Tipul AI: "Slab" sau "Mediu"
        public DateTime GameDate { get; set; }

        public GameResult(string userName, int userScore, int aiScore, string aiType, DateTime gameDate)
        {
            UserName = userName;
            UserScore = userScore;
            AIScore = aiScore;
            AIType = aiType;
            GameDate = gameDate;
        }
    }
}
