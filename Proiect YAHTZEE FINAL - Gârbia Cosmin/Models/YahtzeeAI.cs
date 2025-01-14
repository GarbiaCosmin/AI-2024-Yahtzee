using System;
using System.Collections.Generic;
using System.Linq;

namespace YahtzeeProject.Models
{
    public class YahtzeeAI
    {
        private Random random = new Random();
        private Dictionary<string, int> preTrainedScores;

        public YahtzeeAI()
        {
            // Pre-antrenament (pentru demonstrație, valori hardcodate)
            preTrainedScores = new Dictionary<string, int>
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

        public string PerformTurn(int[] initialDice, Dictionary<string, int?> availableCategories, int maxRolls = 3)
        {
            int[] currentDice = RollAllDice(); 
            int rolls = 1; 

            do
            {
                rolls++;
                string targetCategory = ChooseCategory(currentDice, availableCategories);
                int[] diceToKeep = GetDiceToKeep(currentDice, targetCategory);
                currentDice = RollRemainingDice(currentDice, diceToKeep);
            } while (rolls < maxRolls && ShouldRollAgain(currentDice, availableCategories));

            // Alegem categoria finală
            string chosenCategory = ChooseCategory(currentDice, availableCategories);
            return chosenCategory;
        }

        private int[] RollAllDice()
        {
            return Enumerable.Range(0, 5).Select(_ => random.Next(1, 7)).ToArray();
        }

        private int[] RollRemainingDice(int[] dice, int[] indicesToKeep)
        {
            return dice.Select((value, index) => indicesToKeep.Contains(index) ? value : random.Next(1, 7)).ToArray();
        }

        private bool ShouldRollAgain(int[] dice, Dictionary<string, int?> availableCategories)
        {
            // Simplă logică de decizie: aruncăm din nou dacă există șanse să obținem un scor mai mare
            string bestCategory = ChooseCategory(dice, availableCategories);
            int currentScore = CalculateScore(dice, bestCategory);
            return currentScore < preTrainedScores[bestCategory] / 2; // Prag arbitrar: jumătate din scorul maxim posibil
        }

        public virtual int[] GetDiceToKeep(int[] dice, string targetCategory)
        {
            // Strategia de păstrare a zarurilor bazată pe categoria țintă
            switch (targetCategory)
            {
                case "Ones": return KeepSpecificNumber(dice, 1);
                case "Twos": return KeepSpecificNumber(dice, 2);
                case "Threes": return KeepSpecificNumber(dice, 3);
                case "Fours": return KeepSpecificNumber(dice, 4);
                case "Fives": return KeepSpecificNumber(dice, 5);
                case "Sixes": return KeepSpecificNumber(dice, 6);
                case "ThreeOfAKind": return KeepBestMatchingGroup(dice, 3);
                case "FourOfAKind": return KeepBestMatchingGroup(dice, 4);
                case "FullHouse": return KeepFullHouse(dice);
                case "SmallStraight":
                case "LargeStraight": return KeepStraight(dice);
                default: return Array.Empty<int>();
            }
        }

        public virtual string ChooseCategory(int[] dice, Dictionary<string, int?> availableCategories)
        {
            // Găsește scorurile posibile pentru fiecare categorie disponibilă
            var categoryScores = availableCategories
                .Where(c => c.Value == null) // Filtrăm categoriile deja completate
                .Select(c => new
                {
                    Category = c.Key,
                    Score = CalculateScore(dice, c.Key)
                })
                .OrderByDescending(c => c.Score) // Ordonăm după scor descrescător
                .ToList();

            // Alege categoria cu scorul maxim
            return categoryScores.First().Category;
        }

        public int CalculateScore(int[] dice, string category)
        {
            // Calculează scorul pentru o anumită categorie
            switch (category)
            {
                case "Ones": return dice.Count(d => d == 1) * 1;
                case "Twos": return dice.Count(d => d == 2) * 2;
                case "Threes": return dice.Count(d => d == 3) * 3;
                case "Fours": return dice.Count(d => d == 4) * 4;
                case "Fives": return dice.Count(d => d == 5) * 5;
                case "Sixes": return dice.Count(d => d == 6) * 6;
                case "ThreeOfAKind": return HasNOfAKind(dice, 3) ? dice.Sum() : 0;
                case "FourOfAKind": return HasNOfAKind(dice, 4) ? dice.Sum() : 0;
                case "FullHouse": return IsFullHouse(dice) ? 25 : 0;
                case "SmallStraight": return IsSmallStraight(dice) ? 30 : 0;
                case "LargeStraight": return IsLargeStraight(dice) ? 40 : 0;
                case "Yahtzee": return HasNOfAKind(dice, 5) ? 50 : 0;
                case "Chance": return dice.Sum();
                default: return 0;
            }
        }

        // Metode auxiliare pentru logica AI
        private int[] KeepSpecificNumber(int[] dice, int number)
        {
            return dice.Select((d, i) => d == number ? i : -1).Where(i => i != -1).ToArray();
        }

        private int[] KeepBestMatchingGroup(int[] dice, int n)
        {
            var groups = dice.GroupBy(d => d).Where(g => g.Count() >= n).OrderByDescending(g => g.Count());
            return groups.Any() ? groups.First().Select((d, i) => Array.IndexOf(dice, d)).ToArray() : Array.Empty<int>();
        }

        private int[] KeepFullHouse(int[] dice)
        {
            return dice.Distinct().Count() == 2 ? dice.Select((d, i) => i).ToArray() : Array.Empty<int>();
        }

        private int[] KeepStraight(int[] dice)
        {
            return dice.Distinct().OrderBy(d => d).Select((d, i) => i).ToArray();
        }

        private bool HasNOfAKind(int[] dice, int n)
        {
            return dice.GroupBy(d => d).Any(g => g.Count() >= n);
        }

        private bool IsFullHouse(int[] dice)
        {
            var grouped = dice.GroupBy(d => d).Select(g => g.Count()).OrderBy(c => c).ToArray();
            return grouped.Length == 2 && grouped[0] == 2 && grouped[1] == 3;
        }

        private bool IsSmallStraight(int[] dice)
        {
            int[] uniqueDice = dice.Distinct().OrderBy(d => d).ToArray();
            return uniqueDice.Contains(1) && uniqueDice.Contains(2) && uniqueDice.Contains(3) && uniqueDice.Contains(4) ||
                   uniqueDice.Contains(2) && uniqueDice.Contains(3) && uniqueDice.Contains(4) && uniqueDice.Contains(5) ||
                   uniqueDice.Contains(3) && uniqueDice.Contains(4) && uniqueDice.Contains(5) && uniqueDice.Contains(6);
        }

        private bool IsLargeStraight(int[] dice)
        {
            int[] uniqueDice = dice.Distinct().OrderBy(d => d).ToArray();
            return uniqueDice.SequenceEqual(new int[] { 1, 2, 3, 4, 5 }) || uniqueDice.SequenceEqual(new int[] { 2, 3, 4, 5, 6 });
        }
    }
}
