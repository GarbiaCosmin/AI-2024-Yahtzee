namespace YahtzeeProject.Feedback
{
    public class GameReportGenerator
    {
        private MoveTracker moveTracker;
        private MoveAnalyzer moveAnalyzer;

        public GameReportGenerator(MoveTracker moveTracker, MoveAnalyzer moveAnalyzer)
        {
            this.moveTracker = moveTracker;
            this.moveAnalyzer = moveAnalyzer;
        }

        public string GenerateReport()
        {
            var moves = moveTracker.GetMoves();
            var analysis = moveAnalyzer.AnalyzeMoves(moves);

            return string.Join("\n", analysis);
        }
    }
}
