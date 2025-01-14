using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using YahtzeeProject.Models;
using YahtzeeProject.Services;
using YahtzeeProject.UI;
using YahtzeeProject.Feedback;

namespace YahtzeeProject
{
    public partial class GameForm : Form
    {
        private YahtzeeGame game;
        private List<CheckBox> diceCheckBoxes = new List<CheckBox>();
        private Random random = new Random();
        private LeaderboardManager leaderboardManager;
        private ChatMenu chatMenu;
      


        public GameForm()
        {
            InitializeComponent();
            InitializeGame();
            leaderboardManager = new LeaderboardManager(Path.Combine(Environment.CurrentDirectory, "leaderboard.csv"));
            chatMenu = new ChatMenu(this, game.GameState);


        }

        private void UpdateCategoryComboBox()
        {
            comboBoxCategory.Items.Clear(); // Golim lista anterioară
            foreach (var category in game.GameState.UserScoreCard.Keys)
            {
                if (game.GameState.IsCategoryAvailable(category))
                {
                    comboBoxCategory.Items.Add(category);
                }
            }
            comboBoxCategory.SelectedIndex = -1; // Resetăm selecția
        }

        private void InitializeGame()
        {
            AIType selectedAI = ShowAISelectionDialog(); // Alegem tipul de AI

            game = new YahtzeeGame(selectedAI); // Transmitem tipul corect către YahtzeeGame

            diceCheckBoxes.AddRange(new[] { checkBoxDice1, checkBoxDice2, checkBoxDice3, checkBoxDice4, checkBoxDice5 });
            UpdateDiceDisplay();
            InitializeScoreCardGrids();
            UpdateScoreCardDisplays();

            comboBoxCategory.Items.Clear();
            comboBoxCategory.Items.AddRange(game.GameState.UserScoreCard.Keys.ToArray());
            buttonChooseCategory.Enabled = false;
        }

        // Dialog pentru alegerea tipului de AI
        private AIType ShowAISelectionDialog()
        {
            var result = MessageBox.Show(
                "Alege tipul de AI pe care ți-l dorești.\nApăsați Da pentru slab, Nu pentru mediu.",
                "Alegeți AI-ul",
                MessageBoxButtons.YesNo
            );

            AIType selectedAI = result == DialogResult.Yes ? AIType.Weak : AIType.Medium;
            return selectedAI;
        }



        // Funcție pentru inițializarea tabelelor de scor
        private void InitializeScoreCardGrids()
        {
            userScoreCardGrid.Columns.Clear();
            userScoreCardGrid.Columns.Add("Category", "Category");
            userScoreCardGrid.Columns.Add("UserScore", "User Score");
            aiScoreCardGrid.Columns.Clear();
            aiScoreCardGrid.Columns.Add("Category", "Category");
            aiScoreCardGrid.Columns.Add("AIScore", "AI Score");
        }

        // Funcție pentru afișarea zarurilor
        private void UpdateDiceDisplay()
        {
            for (int i = 0; i < game.GameState.Dice.Length; i++)
            {
                diceCheckBoxes[i].Text = game.GameState.Dice[i].ToString();
                diceCheckBoxes[i].Checked = false; // Debifăm toate zarurile
            }
        }

        // Funcție pentru actualizarea tabelelor de scor
        private void UpdateScoreCardDisplays()
        {
            userScoreCardGrid.Rows.Clear();
            aiScoreCardGrid.Rows.Clear();

            // Actualizăm tabelele de scor
            foreach (var entry in game.GameState.UserScoreCard)
            {
                userScoreCardGrid.Rows.Add(entry.Key, entry.Value?.ToString() ?? "N/A");
            }
            foreach (var entry in game.GameState.AIScoreCard)
            {
                aiScoreCardGrid.Rows.Add(entry.Key, entry.Value?.ToString() ?? "N/A");
            }

            // Calculăm scorurile totale
            int userTotalScore = game.GameState.GetTotalScore(game.GameState.UserScoreCard);
            int aiTotalScore = game.GameState.GetTotalScore(game.GameState.AIScoreCard);

            // Actualizăm etichetele pentru scorurile live
            labelUserScore.Text = $"User Total Score: {userTotalScore}";
            labelAIScore.Text = $"AI Total Score: {aiTotalScore}";
        }


        // Funcția pentru butonul de aruncare zaruri
        private void buttonRollDice_Click(object sender, EventArgs e)
        {
            // Verificăm dacă utilizatorul încearcă să păstreze un zar cu valoarea 0
            if (diceCheckBoxes.Any(cb => cb.Checked && game.GameState.Dice[diceCheckBoxes.IndexOf(cb)] == 0))
            {
                MessageBox.Show("Nu poți păstra zaruri cu valoarea 0.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Verificăm dacă toate zarurile sunt bifate
            if (diceCheckBoxes.All(cb => cb.Checked))
            {
                MessageBox.Show("Nu poți păstra toate zarurile. Selectează mai puține zaruri.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                foreach (var cb in diceCheckBoxes)
                {
                    cb.Checked = false; // Resetează selecția zarurilor
                }
                return;
            }

            // Continuăm cu logica obișnuită de aruncare a zarurilor
            if (game.GameState.RollsRemaining > 0)
            {
                int[] diceToKeep = diceCheckBoxes
                    .Select((cb, index) => cb.Checked ? index : -1)
                    .Where(index => index != -1)
                    .ToArray();

                game.RollDice(diceToKeep);
                UpdateDiceDisplay();
                buttonChooseCategory.Enabled = true; // Activăm butonul de alegere a categoriei după prima aruncare
                if (game.GameState.RollsRemaining == 0)
                {
                    buttonRollDice.Enabled = false; // Dezactivăm butonul de aruncare a zarurilor dacă nu mai avem aruncări
                }
            }
        }

        // Funcția pentru alegerea categoriei de scor de către jucător
        private void buttonChooseCategory_Click(object sender, EventArgs e)
        {
            string selectedCategory = comboBoxCategory.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedCategory) && game.GameState.IsCategoryAvailable(selectedCategory))
            {
                game.UpdateScore(selectedCategory);
                UpdateScoreCardDisplays();
                comboBoxCategory.Items.Remove(selectedCategory);
                comboBoxCategory.SelectedIndex = -1;
                buttonChooseCategory.Enabled = false;
                buttonRollDice.Enabled = true;

                // După turul jucătorului, AI-ul joacă
              //  MessageBox.Show("AI is taking its turn...");
                game.AITurn();
                UpdateScoreCardDisplays();
                UpdateDiceDisplay();

                // Verificăm dacă jocul s-a terminat
                if (game.GameState.IsGameOver())
                {
                    EndGame(); // Înlocuiește codul curent cu apelul metodei EndGame
                }

            }
            else
            {
                MessageBox.Show("Please select a valid category.");
            }
        }


        // Funcția pentru turul AI-ului
        private void AITurn()
        {
            game.AITurn();
            UpdateScoreCardDisplays();
            UpdateDiceDisplay(); // Actualizăm afișarea zarurilor după turul AI-ului
            if (game.GameState.IsGameOver())
            {
                int userTotalScore = game.GameState.GetTotalScore(game.GameState.UserScoreCard);
                int aiTotalScore = game.GameState.GetTotalScore(game.GameState.AIScoreCard);
                string winner = userTotalScore > aiTotalScore ? "User" : "AI";
                MessageBox.Show($"Game over! {winner} wins with a score of {Math.Max(userTotalScore, aiTotalScore)}!");
                InitializeGame();
            }
        }
        private void EndGame()
        {
            int userScore = game.GameState.GetTotalScore(game.GameState.UserScoreCard);
            int aiScore = game.GameState.GetTotalScore(game.GameState.AIScoreCard);

            string aiType = game.AIType.ToString(); // Obține tipul corect de AI

            string userName = Prompt.ShowDialog("Introduceți numele:", "Salvare Scor");
            if (string.IsNullOrWhiteSpace(userName)) userName = "Anonim";

            // Salvează rezultatul cu tipul corect de AI
            leaderboardManager.SaveResult(new GameResult(userName, userScore, aiScore, aiType, DateTime.Now));

            string winner = userScore > aiScore ? "User" : "AI";
            MessageBox.Show($"Game over! {winner} wins with a score of {Math.Max(userScore, aiScore)}!");

            var moveAnalyzer = new MoveAnalyzer();
            var reportGenerator = new GameReportGenerator(game.MoveTracker, moveAnalyzer);

            string report = reportGenerator.GenerateReport();

            MessageBox.Show(report, "Raport Final", MessageBoxButtons.OK, MessageBoxIcon.Information);


            InitializeGame();
        }



        private void buttonShowLeaderboard_Click(object sender, EventArgs e)
        {
            var results = leaderboardManager.LoadResults();
            string leaderboardContent = string.Join(Environment.NewLine, results.Select(r =>
                $"{r.UserName} - Scor Utilizator: {r.UserScore}, Scor AI: {r.AIScore}, AI: {r.AIType}, Data: {r.GameDate:yyyy-MM-dd HH:mm}"));
            MessageBox.Show(leaderboardContent, "Istoric jocuri");
        }



        private void InitializeComponent()
        {
            this.userScoreCardGrid = new System.Windows.Forms.DataGridView();
            this.aiScoreCardGrid = new System.Windows.Forms.DataGridView();
            this.checkBoxDice1 = new System.Windows.Forms.CheckBox();
            this.checkBoxDice2 = new System.Windows.Forms.CheckBox();
            this.checkBoxDice3 = new System.Windows.Forms.CheckBox();
            this.checkBoxDice4 = new System.Windows.Forms.CheckBox();
            this.checkBoxDice5 = new System.Windows.Forms.CheckBox();
            this.buttonRollDice = new System.Windows.Forms.Button();
            this.buttonChooseCategory = new System.Windows.Forms.Button();
            this.comboBoxCategory = new System.Windows.Forms.ComboBox();
            this.labelUser = new System.Windows.Forms.Label();
            this.labelAI = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.userScoreCardGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aiScoreCardGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // userScoreCardGrid
            // 
            this.userScoreCardGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.userScoreCardGrid.Location = new System.Drawing.Point(12, 32);
            this.userScoreCardGrid.Name = "userScoreCardGrid";
            this.userScoreCardGrid.Size = new System.Drawing.Size(240, 150);
            this.userScoreCardGrid.TabIndex = 0;
            // 
            // aiScoreCardGrid
            // 
            this.aiScoreCardGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.aiScoreCardGrid.Location = new System.Drawing.Point(258, 32);
            this.aiScoreCardGrid.Name = "aiScoreCardGrid";
            this.aiScoreCardGrid.Size = new System.Drawing.Size(240, 150);
            this.aiScoreCardGrid.TabIndex = 1;
            // 
            // checkBoxDice1
            this.checkBoxDice1.AutoSize = true;
            this.checkBoxDice1.Location = new System.Drawing.Point(12, 200);
            this.checkBoxDice1.Name = "checkBoxDice1";
            this.checkBoxDice1.Size = new System.Drawing.Size(65, 19);
            this.checkBoxDice1.TabIndex = 2;
            this.checkBoxDice1.Text = "Zar 1";
            this.checkBoxDice1.UseVisualStyleBackColor = true;
            this.checkBoxDice1.ForeColor = System.Drawing.Color.White; // Schimbă textul în alb

            // checkBoxDice2
            this.checkBoxDice2.AutoSize = true;
            this.checkBoxDice2.Location = new System.Drawing.Point(12, 225);
            this.checkBoxDice2.Name = "checkBoxDice2";
            this.checkBoxDice2.Size = new System.Drawing.Size(65, 19);
            this.checkBoxDice2.TabIndex = 3;
            this.checkBoxDice2.Text = "Zar 2";
            this.checkBoxDice2.UseVisualStyleBackColor = true;
            this.checkBoxDice2.ForeColor = System.Drawing.Color.White; // Schimbă textul în alb

            // checkBoxDice3
            this.checkBoxDice3.AutoSize = true;
            this.checkBoxDice3.Location = new System.Drawing.Point(12, 250);
            this.checkBoxDice3.Name = "checkBoxDice3";
            this.checkBoxDice3.Size = new System.Drawing.Size(65, 19);
            this.checkBoxDice3.TabIndex = 4;
            this.checkBoxDice3.Text = "Zar 3";
            this.checkBoxDice3.UseVisualStyleBackColor = true;
            this.checkBoxDice3.ForeColor = System.Drawing.Color.White; // Schimbă textul în alb

            // checkBoxDice4
            this.checkBoxDice4.AutoSize = true;
            this.checkBoxDice4.Location = new System.Drawing.Point(12, 275);
            this.checkBoxDice4.Name = "checkBoxDice4";
            this.checkBoxDice4.Size = new System.Drawing.Size(65, 19);
            this.checkBoxDice4.TabIndex = 5;
            this.checkBoxDice4.Text = "Zar 4";
            this.checkBoxDice4.UseVisualStyleBackColor = true;
            this.checkBoxDice4.ForeColor = System.Drawing.Color.White; // Schimbă textul în alb

            // checkBoxDice5
            this.checkBoxDice5.AutoSize = true;
            this.checkBoxDice5.Location = new System.Drawing.Point(12, 300);
            this.checkBoxDice5.Name = "checkBoxDice5";
            this.checkBoxDice5.Size = new System.Drawing.Size(65, 19);
            this.checkBoxDice5.TabIndex = 6;
            this.checkBoxDice5.Text = "Zar 5";
            this.checkBoxDice5.UseVisualStyleBackColor = true;
            this.checkBoxDice5.ForeColor = System.Drawing.Color.White; // Schimbă textul în alb
            //
            // buttonRollDice
            // 
            this.buttonRollDice.Location = new System.Drawing.Point(100, 200);
            this.buttonRollDice.Name = "buttonRollDice";
            this.buttonRollDice.Size = new System.Drawing.Size(75, 23);
            this.buttonRollDice.TabIndex = 7;
            this.buttonRollDice.Text = "Aruncă Zarurile";
            this.buttonRollDice.UseVisualStyleBackColor = true;
            this.buttonRollDice.Click += new System.EventHandler(this.buttonRollDice_Click);
            // 
            // buttonChooseCategory
            // 
            this.buttonChooseCategory.Location = new System.Drawing.Point(100, 230);
            this.buttonChooseCategory.Name = "buttonChooseCategory";
            this.buttonChooseCategory.Size = new System.Drawing.Size(75, 23);
            this.buttonChooseCategory.TabIndex = 8;
            this.buttonChooseCategory.Text = "Alege Categorie";
            this.buttonChooseCategory.UseVisualStyleBackColor = true;
            this.buttonChooseCategory.Click += new System.EventHandler(this.buttonChooseCategory_Click);
            // 
            // comboBoxCategory
            // 
            this.comboBoxCategory.FormattingEnabled = true;
            this.comboBoxCategory.Location = new System.Drawing.Point(100, 260);
            this.comboBoxCategory.Name = "comboBoxCategory";
            this.comboBoxCategory.Size = new System.Drawing.Size(121, 23);
            this.comboBoxCategory.TabIndex = 9;
            // 
            // labelUser
            this.labelUser.AutoSize = true;
            this.labelUser.Location = new System.Drawing.Point(12, 12);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(33, 15);
            this.labelUser.TabIndex = 10;
            this.labelUser.Text = "User";
            this.labelUser.ForeColor = System.Drawing.Color.White; // Schimbăm culoarea textului în alb

            // labelAI
            this.labelAI.AutoSize = true;
            this.labelAI.Location = new System.Drawing.Point(258, 12);
            this.labelAI.Name = "labelAI";
            this.labelAI.Size = new System.Drawing.Size(19, 15);
            this.labelAI.TabIndex = 11;
            this.labelAI.Text = "AI";
            this.labelAI.ForeColor = System.Drawing.Color.White; // Schimbăm culoarea textului în alb

            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.BackColor = System.Drawing.Color.FromArgb(0, 128, 0);
            this.Controls.Add(this.labelAI);
            this.Controls.Add(this.labelUser);
            this.Controls.Add(this.comboBoxCategory);
            this.Controls.Add(this.buttonChooseCategory);
            this.Controls.Add(this.buttonRollDice);
            this.Controls.Add(this.checkBoxDice5);
            this.Controls.Add(this.checkBoxDice4);
            this.Controls.Add(this.checkBoxDice3);
            this.Controls.Add(this.checkBoxDice2);
            this.Controls.Add(this.checkBoxDice1);
            this.Controls.Add(this.aiScoreCardGrid);
            this.Controls.Add(this.userScoreCardGrid);
            this.Name = "GameForm";
            this.Text = "Yahtzee";
            ((System.ComponentModel.ISupportInitialize)(this.userScoreCardGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aiScoreCardGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
            // Adăugăm două noi Label-uri pentru scoruri live
            this.labelUserScore = new System.Windows.Forms.Label();
            this.labelAIScore = new System.Windows.Forms.Label();
            // 
            // Configurăm labelUserScore
            this.labelUserScore.AutoSize = true; // Activăm AutoSize pentru a calcula dimensiunea în funcție de text
            this.labelUserScore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft; // Text aliniat la stânga
            this.labelUserScore.Location = new System.Drawing.Point(30, 440); // Poziționare corectă
            this.labelUserScore.Name = "labelUserScore";
            this.labelUserScore.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold); // Font mare și bold
            this.labelUserScore.ForeColor = System.Drawing.Color.White; // Text alb pentru contrast
            this.labelUserScore.BackColor = System.Drawing.Color.DarkGreen; // Fundal pentru contrast
            this.labelUserScore.Text = "User Total Score: 0"; // Text inițial

            // Configurăm labelAIScore
            this.labelAIScore.AutoSize = true; // Activăm AutoSize pentru a calcula dimensiunea în funcție de text
            this.labelAIScore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft; // Text aliniat la stânga
            this.labelAIScore.Location = new System.Drawing.Point(400, 440); // Poziționare corectă
            this.labelAIScore.Name = "labelAIScore";
            this.labelAIScore.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold); // Font mare și bold
            this.labelAIScore.ForeColor = System.Drawing.Color.White; // Text alb pentru contrast
            this.labelAIScore.BackColor = System.Drawing.Color.DarkGreen; // Fundal pentru contrast
            this.labelAIScore.Text = "AI Total Score: 0"; // Text inițial

            // Adăugăm etichetele direct la formular
            this.Controls.Add(this.labelUserScore);
            this.Controls.Add(this.labelAIScore);
            //

            this.buttonLeaderboard = new System.Windows.Forms.Button();
            this.buttonLeaderboard.Location = new System.Drawing.Point(113, 385);
            this.buttonLeaderboard.Name = "buttonLeaderboard";
            this.buttonLeaderboard.Size = new System.Drawing.Size(100, 30);
            this.buttonLeaderboard.Text = "Istoric jocuri";
            this.buttonLeaderboard.UseVisualStyleBackColor = true;
            this.buttonLeaderboard.Click += new System.EventHandler(this.buttonShowLeaderboard_Click);
            this.Controls.Add(this.buttonLeaderboard);



        }

        private System.Windows.Forms.DataGridView userScoreCardGrid;
        private System.Windows.Forms.DataGridView aiScoreCardGrid;
        private System.Windows.Forms.CheckBox checkBoxDice1;
        private System.Windows.Forms.CheckBox checkBoxDice2;
        private System.Windows.Forms.CheckBox checkBoxDice3;
        private System.Windows.Forms.CheckBox checkBoxDice4;
        private System.Windows.Forms.CheckBox checkBoxDice5;
        private System.Windows.Forms.Button buttonRollDice;
        private System.Windows.Forms.Button buttonChooseCategory;
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.Label labelAI;
        private System.Windows.Forms.Label labelUserScore;
        private System.Windows.Forms.Label labelAIScore;
        private System.Windows.Forms.Button buttonLeaderboard;
    }
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 10, Top = 10, Text = text };
            TextBox textBox = new TextBox() { Left = 10, Top = 30, Width = 260 };
            Button confirmation = new Button() { Text = "OK", Left = 200, Top = 70, Width = 75 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;
            prompt.ShowDialog();
            return textBox.Text;
        }
    }

}