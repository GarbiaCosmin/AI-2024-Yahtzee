using System.Collections.Generic;
using System.Linq;

namespace YahtzeeProject.Models
{
    public class GameState
    {
        // Zarurile (5 zaruri)
        public int[] Dice { get; set; } = new int[5];

        // Scorul pentru fiecare jucător
        public Dictionary<string, int?> UserScoreCard { get; set; }
        public Dictionary<string, int?> AIScoreCard { get; set; }

        // Jucătorul curent ("User" sau "AI")
        public string CurrentPlayer { get; set; }

        // Aruncări rămase per rundă
        public int RollsRemaining { get; set; } = 3;

        // Constructor pentru inițializarea stării jocului
        public GameState()
        {
            InitializeGame();
        }

        // Funcția de inițializare a jocului
        public void InitializeGame()
        {
            Dice = new int[5] { 0, 0, 0, 0, 0 };
            UserScoreCard = new Dictionary<string, int?>
            {
                { "Ones", null },
                { "Twos", null },
                { "Threes", null },
                { "Fours", null },
                { "Fives", null },
                { "Sixes", null },
                { "ThreeOfAKind", null },
                { "FourOfAKind", null },
                { "FullHouse", null },
                { "SmallStraight", null },
                { "LargeStraight", null },
                { "Yahtzee", null },
                { "Chance", null }
            };
            AIScoreCard = new Dictionary<string, int?>
            {
                { "Ones", null },
                { "Twos", null },
                { "Threes", null },
                { "Fours", null },
                { "Fives", null },
                { "Sixes", null },
                { "ThreeOfAKind", null },
                { "FourOfAKind", null },
                { "FullHouse", null },
                { "SmallStraight", null },
                { "LargeStraight", null },
                { "Yahtzee", null },
                { "Chance", null }
            };
         
            CurrentPlayer = "User";
            RollsRemaining = 3;
        }

        // Funcția pentru verificarea dacă jocul s-a terminat
        public bool IsGameOver()
        {
            return UserScoreCard.Values.All(value => value.HasValue) && AIScoreCard.Values.All(value => value.HasValue);
        }

        // Funcția care trece la următorul jucător
        public void SwitchPlayer()
        {
            CurrentPlayer = (CurrentPlayer == "User") ? "AI" : "User";
        }

        // Funcția pentru verificarea disponibilității unei categorii
        public bool IsCategoryAvailable(string category)
        {
            if (CurrentPlayer == "User")
            {
                return UserScoreCard[category] == null;
            }
            else
            {
                return AIScoreCard[category] == null;
            }
        }

        // Funcția pentru obținerea scorului total
        public int GetTotalScore(Dictionary<string, int?> scoreCard)
        {
            return scoreCard.Values.Where(v => v.HasValue).Sum(v => v.Value);
        }
    }
}
