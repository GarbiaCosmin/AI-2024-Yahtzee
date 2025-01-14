namespace YahtzeeProject.Feedback
{
    public class MoveAnalyzer
    {
        private Dictionary<string, int> maxScores;

        public MoveAnalyzer()
        {
            maxScores = new Dictionary<string, int>
            {
                { "Ones", 5 },
                { "Twos", 10 },
                { "Threes", 15 },
                { "Fours", 20 },
                { "Fives", 25 },
                { "Sixes", 30 },
                { "ThreeOfAKind", 25 },
                { "FourOfAKind", 30 },
                { "FullHouse", 25 },
                { "SmallStraight", 30 },
                { "LargeStraight", 40 },
                { "Yahtzee", 50 },
                { "Chance", 20 }
            };
        }

        public List<string> AnalyzeMoves(List<UserMove> moves)
        {
            var feedback = new List<string>();

            foreach (var move in moves)
            {
                if (maxScores.ContainsKey(move.Category))
                {
                    int maxScore = maxScores[move.Category];
                    if (move.Score >= maxScore * 0.8) // Mutare bună: scor >= 80% din maxim
                    {
                        feedback.Add($"Mutare bună: {move.Category} - Scor {move.Score}/{maxScore}");
                    }
                    else
                    {
                        feedback.Add($"Mutare slabă: {move.Category} - Scor {move.Score}/{maxScore}");
                    }
                }
            }

            return feedback;
        }
    }
}
