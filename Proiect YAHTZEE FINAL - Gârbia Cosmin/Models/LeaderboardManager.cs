using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YahtzeeProject.Services
{
    public class LeaderboardManager
    {
        private readonly string filePath;

        public LeaderboardManager(string filePath)
        {
            this.filePath = filePath;
            if (!File.Exists(filePath))
            {
                using (var stream = File.Create(filePath)) { }
                // Adăugăm coloana AIType
                File.AppendAllText(filePath, "UserName,UserScore,AIScore,AIType,GameDate" + Environment.NewLine);
            }
        }

        public void SaveResult(GameResult result)
        {
            // Linie completă de rezultat
            var line = $"{result.UserName},{result.UserScore},{result.AIScore},{result.AIType},{result.GameDate:O}";
            File.AppendAllText(filePath, line + Environment.NewLine);
        }


        public List<GameResult> LoadResults()
        {
            if (!File.Exists(filePath)) return new List<GameResult>();

            var lines = File.ReadAllLines(filePath).Skip(1); // Sărim peste antet
            return lines
                .Where(line => !string.IsNullOrWhiteSpace(line)) // Ignorăm liniile goale
                .Select(line =>
                {
                    var parts = line.Split(',');
                    if (parts.Length < 5) // Dacă lipsesc coloanele
                    {
                        return new GameResult(
                            parts[0], // UserName
                            int.Parse(parts[1]), // UserScore
                            int.Parse(parts[2]), // AIScore
                            "idk", // AIType implicit pentru date vechi
                            DateTime.Parse(parts.Length > 3 ? parts[3] : DateTime.Now.ToString()) // Data implicită
                        );
                    }

                    return new GameResult(
                        parts[0], // UserName
                        int.Parse(parts[1]), // UserScore
                        int.Parse(parts[2]), // AIScore
                        parts[3], // AIType
                        DateTime.Parse(parts[4]) // GameDate
                    );
                })
                .ToList();
        }

        public Dictionary<string, List<string>> GenerateStatisticsGrouped()
        {
            var results = LoadResults();
            if (!results.Any())
                return new Dictionary<string, List<string>> { { "Nu există date suficiente pentru statistici.", new List<string>() } };

            // Grupăm rezultatele după numele utilizatorului
            var groupedStatistics = results
                .GroupBy(r => r.UserName)
                .ToDictionary(
                    g => g.Key,
                    g => new List<string>
                    {
                        $"Jocuri câștigate de utilizator: {g.Count(r => r.UserScore > r.AIScore)}",
                        $"Jocuri câștigate de AI: {g.Count(r => r.AIScore > r.UserScore)}",
                        $"Scor maxim utilizator: {g.Max(r => r.UserScore)}",
                        $"Tipuri de AI întâlnite: {string.Join(", ", g.Select(r => r.AIType).Distinct())}"
                    }
                );

            return groupedStatistics;
        }
        public Dictionary<string, List<int>> GetPlayerScores(string playerName)
        {
            var results = LoadResults()
                .Where(r => r.UserName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                .GroupBy(r => r.AIType)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.UserScore).ToList()
                );

            return results;
        }

    }
}

