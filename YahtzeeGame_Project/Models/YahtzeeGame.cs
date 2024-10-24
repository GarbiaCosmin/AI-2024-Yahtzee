using System;
using System.Collections.Generic;
using System.Linq;

namespace YahtzeeProject.Models
{
    public class YahtzeeGame
    {
        public GameState GameState { get; private set; }
        private Random random = new Random(); // Obiect Random

        public YahtzeeGame()
        {
            GameState = new GameState();
        }

        // Funcția de tranziție (aruncare zaruri)
        public void RollDice(int[] diceToKeep)
        {
            for (int i = 0; i < GameState.Dice.Length; i++)
            {
                if (!diceToKeep.Contains(i))
                {
                    GameState.Dice[i] = random.Next(1, 7); // Zarurile au valori între 1 și 6
                }
            }
            GameState.RollsRemaining--;
        }

        // Funcția pentru AI: aruncare zaruri aleatorie
        public void AIRollDice()
        {
            int[] diceToKeep = new int[5];
            for (int i = 0; i < GameState.Dice.Length; i++)
            {
                diceToKeep[i] = random.Next(0, 2); // AI decide aleatoriu să păstreze sau nu zarul
            }
            RollDice(diceToKeep);
        }

        // Funcția pentru AI: alegerea aleatorie a unei categorii necompletate
        public string AIChooseCategory()
        {
            var availableCategories = GameState.AIScoreCard.Where(c => c.Value == null).Select(c => c.Key).ToList();
            if (availableCategories.Count == 0)
            {
                throw new InvalidOperationException("Nu există categorii disponibile.");
            }
            int chosenIndex = random.Next(availableCategories.Count);
            return availableCategories[chosenIndex];
        }

        // Funcția care execută turul AI-ului
        public void AITurn()
        {
            GameState.CurrentPlayer = "AI"; // Setăm jucătorul curent la AI
            GameState.RollsRemaining = 3; // Resetăm numărul de aruncări pentru AI
            while (GameState.RollsRemaining > 0)
            {
                AIRollDice();
            }
            // Alege o categorie aleatorie și actualizează scorul
            string chosenCategory = AIChooseCategory();
            UpdateScore(chosenCategory);
            GameState.CurrentPlayer = "User"; // Resetăm jucătorul curent la User după turul AI-ului
        }

        // Funcția de actualizare a scorului
        public void UpdateScore(string category)
        {
            int score = 0;
            switch (category)
            {
                case "Ones":
                    score = CalculateNumberScore(1);
                    break;
                case "Twos":
                    score = CalculateNumberScore(2);
                    break;
                case "Threes":
                    score = CalculateNumberScore(3);
                    break;
                case "Fours":
                    score = CalculateNumberScore(4);
                    break;
                case "Fives":
                    score = CalculateNumberScore(5);
                    break;
                case "Sixes":
                    score = CalculateNumberScore(6);
                    break;
                case "ThreeOfAKind":
                    if (HasNOfAKind(3)) score = GameState.Dice.Sum();
                    break;
                case "FourOfAKind":
                    if (HasNOfAKind(4)) score = GameState.Dice.Sum();
                    break;
                case "FullHouse":
                    if (IsFullHouse()) score = 25;
                    break;
                case "SmallStraight":
                    if (IsSmallStraight()) score = 30;
                    break;
                case "LargeStraight":
                    if (IsLargeStraight()) score = 40;
                    break;
                case "Yahtzee":
                    if (HasNOfAKind(5)) score = 50;
                    break;
                case "Chance":
                    score = GameState.Dice.Sum();
                    break;
            }

            if (GameState.IsCategoryAvailable(category))
            {
                if (GameState.CurrentPlayer == "User")
                {
                    GameState.UserScoreCard[category] = score;
                }
                else
                {
                    GameState.AIScoreCard[category] = score;
                }
                GameState.RollsRemaining = 3; // Resetăm numărul de aruncări pentru următoarea tură
            }
        }

        // Funcții auxiliare pentru calcularea scorului
        private int CalculateNumberScore(int number)
        {
            return GameState.Dice.Where(d => d == number).Sum();
        }

        private bool HasNOfAKind(int n)
        {
            return GameState.Dice.GroupBy(d => d).Any(g => g.Count() >= n);
        }

        private bool IsFullHouse()
        {
            var grouped = GameState.Dice.GroupBy(d => d).Select(g => g.Count()).OrderBy(c => c).ToArray();
            return grouped.Length == 2 && grouped[0] == 2 && grouped[1] == 3;
        }

        private bool IsSmallStraight()
        {
            int[] uniqueDice = GameState.Dice.Distinct().OrderBy(d => d).ToArray();
            return uniqueDice.Contains(1) && uniqueDice.Contains(2) && uniqueDice.Contains(3) && uniqueDice.Contains(4) ||
                   uniqueDice.Contains(2) && uniqueDice.Contains(3) && uniqueDice.Contains(4) && uniqueDice.Contains(5) ||
                   uniqueDice.Contains(3) && uniqueDice.Contains(4) && uniqueDice.Contains(5) && uniqueDice.Contains(6);
        }

        private bool IsLargeStraight()
        {
            int[] uniqueDice = GameState.Dice.Distinct().OrderBy(d => d).ToArray();
            return uniqueDice.SequenceEqual(new int[] { 1, 2, 3, 4, 5 }) || uniqueDice.SequenceEqual(new int[] { 2, 3, 4, 5, 6 });
        }
    }
}


/*
 * Categoriile de Numere (Ones, Twos, Threes, Fours, Fives, Sixes)
Ones (Unu): Suma zarurilor care arată 1.
Twos (Doi): Suma zarurilor care arată 2.
Threes (Trei): Suma zarurilor care arată 3.
Fours (Patru): Suma zarurilor care arată 4.
Fives (Cinci): Suma zarurilor care arată 5.
Sixes (Șase): Suma zarurilor care arată 6.
 
Categoriile de Combinații
Three of a Kind (Trei de un fel): Dacă există cel puțin trei zaruri cu aceeași valoare, se adună toate zarurile.
Four of a Kind (Patru de un fel): Dacă există cel puțin patru zaruri cu aceeași valoare, se adună toate zarurile.
Full House (Full House): Dacă există trei zaruri de un fel și două zaruri de alt fel (ex. trei 2 și două 5), se acordă 25 de puncte.
Small Straight (Mică Stradă): Dacă există patru zaruri consecutive (ex. 1-2-3-4 sau 2-3-4-5), se acordă 30 de puncte.
Large Straight (Mare Stradă): Dacă există cinci zaruri consecutive (ex. 1-2-3-4-5 sau 2-3-4-5-6), se acordă 40 de puncte.
Yahtzee (Yahtzee): Dacă toate cele cinci zaruri au aceeași valoare, se acordă 50 de puncte.
Chance (Șansă): Se adună toate zarurile, indiferent de combinație.
 
 
 */