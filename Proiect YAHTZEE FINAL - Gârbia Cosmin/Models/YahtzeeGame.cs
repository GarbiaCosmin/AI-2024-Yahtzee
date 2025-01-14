using System;
using System.Linq;
using YahtzeeProject.Feedback;

public enum AIType
{
    Weak,   // Random AI
    Medium  // AI cu strategie mai bună
}


namespace YahtzeeProject.Models
{
    public class YahtzeeGame
    {
        public MoveTracker MoveTracker { get; private set; }
        public AIType AIType { get; private set; } // Proprietate pentru tipul de AI
        public GameState GameState { get; private set; }
        private Random random = new Random();
        private YahtzeeAI ai; // Instanță AI
        private AIType aiType;

        public YahtzeeGame(AIType aiType)
        {
            GameState = new GameState();
            this.AIType = aiType;
            MoveTracker = new MoveTracker(); // Initialize MoveTracker

            // Alegem tipul de AI
            if (aiType == AIType.Medium)
            {
                ai = new YahtzeeAI(); // AI mediu
            }
            else
            {
                ai = new YahtzeeRandomAI(); // AI slab
            }
        }
        // Funcția de tranziție (aruncare zaruri pentru utilizator)
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

        // Funcția care execută turul AI-ului
        public void AITurn()
        {
            GameState.CurrentPlayer = "AI"; // Setăm jucătorul curent la AI
            GameState.RollsRemaining = 3; // Resetăm numărul de aruncări pentru AI

            // Aruncări multiple până la consumarea tururilor
            while (GameState.RollsRemaining > 0)
            {
                // Alegem categoria optimă pentru AI
                var bestCategory = ai.ChooseCategory(GameState.Dice, GameState.AIScoreCard);
                // Zarurile de păstrat pentru categoria țintă
                var diceToKeep = ai.GetDiceToKeep(GameState.Dice, bestCategory);
                // Aruncăm zarurile care nu sunt păstrate
                RollDice(diceToKeep);
            }

            // După aruncările finale, alegem categoria optimă
            var finalCategory = ai.ChooseCategory(GameState.Dice, GameState.AIScoreCard);
            UpdateScore(finalCategory);

            // Resetăm la jucătorul User pentru următoarea rundă
            GameState.CurrentPlayer = "User";
        }

        // Funcția de actualizare a scorului
        public void UpdateScore(string category)
        {
            int score = ai.CalculateScore(GameState.Dice, category);

            if (GameState.IsCategoryAvailable(category))
            {
                if (GameState.CurrentPlayer == "User")
                {
                    GameState.UserScoreCard[category] = score;
                    MoveTracker.RecordMove(category, score);
                }
                else
                {
                    GameState.AIScoreCard[category] = score;
                }
                GameState.RollsRemaining = 3; // Resetăm numărul de aruncări pentru următoarea tură
            }
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