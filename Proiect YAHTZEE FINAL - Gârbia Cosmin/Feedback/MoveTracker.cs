namespace YahtzeeProject.Feedback
{
    public class MoveTracker
    {
        private List<UserMove> userMoves;

        public MoveTracker()
        {
            userMoves = new List<UserMove>();
        }

        public void RecordMove(string category, int score)
        {
            userMoves.Add(new UserMove(category, score));
        }

        public List<UserMove> GetMoves()
        {
            return userMoves;
        }
    }

    public class UserMove
    {
        public string Category { get; set; }
        public int Score { get; set; }

        public UserMove(string category, int score)
        {
            Category = category;
            Score = score;
        }
    }
}
