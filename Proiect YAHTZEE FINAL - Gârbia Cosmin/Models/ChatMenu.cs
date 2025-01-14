using System;
using System.Drawing;
using System.Windows.Forms;
using YahtzeeProject.Services;
using System.Windows.Forms.DataVisualization.Charting;
using YahtzeeProject.Feedback;
using YahtzeeProject.Models;


namespace YahtzeeProject.UI
{
    public class ChatMenu
    {
        private Button buttonChat; // Butonul principal "Chat"
        private Form parentForm; // Referința către formularul principal
        private Form chatForm; // Formularul deschis pentru meniul "Chat"
        private int[] currentDiceConfig = new int[5]; // Configurația actuală a zarurilor
        private List<string> availableCategories = new List<string>(); // Categoriile disponibile
        private int userScore = 0; // Scorul utilizatorului
        private int aiScore = 0; // Scorul AI-ului

        public ChatMenu(Form form)
        {
            parentForm = form;
            InitializeChatComponents();
        }

        private readonly GameState gameState;

        public ChatMenu(Form form, GameState gameState)
        {
            parentForm = form;
            this.gameState = gameState;
            InitializeChatComponents();
        }
        private void InitializeChatComponents()
        {
            // Butonul principal "Chat"
            buttonChat = new Button
            {
                Text = "Chat",
                Location = new Point(580, 45), // Poziția butonului
                Size = new Size(80, 200),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            buttonChat.Click += ButtonChat_Click;
            parentForm.Controls.Add(buttonChat); // Adaugăm butonul pe formularul principal
        }

        private void ButtonChat_Click(object sender, EventArgs e)
        {
            // Creăm formularul "Chat" când butonul este apăsat
            chatForm = new Form
            {
                Text = "Chat Menu",
                FormBorderStyle = FormBorderStyle.None, // Fără margini
                BackColor = Color.DarkGreen,
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(700, 600), // Dimensiune personalizată
                TopMost = false // Se afișează deasupra
            };
            // Adaugă un panou pentru mutare
            Panel dragPanel = new Panel
            {
                Size = new Size(chatForm.Width, 30), // Dimensiune pe lățimea ferestrei
                Dock = DockStyle.Top, // Fixat sus
                BackColor = Color.LightGray // Culoare pentru identificare (opțional)
            };
            chatForm.Controls.Add(dragPanel);

            // Evenimente pentru mutare
            bool isDragging = false;
            Point dragStartPoint = new Point(0, 0);

            dragPanel.MouseDown += (s, e) =>
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            };

            dragPanel.MouseMove += (s, e) =>
            {
                if (isDragging)
                {
                    chatForm.Left += e.X - dragStartPoint.X;
                    chatForm.Top += e.Y - dragStartPoint.Y;
                }
            };

            dragPanel.MouseUp += (s, e) =>
            {
                isDragging = false;
            };
            // Adăugăm un buton de închidere
            Button buttonClose = new Button
            {
                Text = "X",
                Size = new Size(30, 30),
                Location = new Point(chatForm.Width - 50, 40),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,

            };
            buttonClose.Click += (s, args) => chatForm.Close();
            chatForm.Controls.Add(buttonClose);

            // Adăugăm butonul "Vezi regulamentul"
            Button buttonRegulament = new Button
            {
                Text = "Vezi regulamentul",
                Location = new Point(50, 50),
                Size = new Size(200, 40),
                BackColor = Color.LightGreen
            };
            buttonRegulament.Click += ButtonRegulament_Click;
            chatForm.Controls.Add(buttonRegulament);

            // Adăugăm butonul "Statistici și progres"
            Button buttonStatistici = new Button
            {
                Text = "Statistici și progres",
                Location = new Point(50, 110),
                Size = new Size(200, 40),
                BackColor = Color.LightGreen
            };
            buttonStatistici.Click += ButtonStatistici_Click;
            chatForm.Controls.Add(buttonStatistici);

            // Adăugăm o casetă de text pentru dialog
            TextBox textBoxChat = new TextBox
            {
                Location = new Point(50, 170),
                Size = new Size(500, 100),
                Multiline = true,
                PlaceholderText = "Discuta cu AI-ul tau personilzat..."
            };
            chatForm.Controls.Add(textBoxChat);

            // Adăugăm un buton pentru trimiterea mesajului
            Button buttonSend = new Button
            {
                Text = "Trimite",
                Location = new Point(560, 170),
                Size = new Size(100, 40),
                BackColor = Color.LightGreen
            };
            chatForm.Controls.Add(buttonSend);

            // Adăugăm o zonă pentru afișarea răspunsurilor
            TextBox responseBox = new TextBox
            {
                Location = new Point(50, 300),
                Size = new Size(600, 200),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };
            chatForm.Controls.Add(responseBox);

            string apiKey = "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"; // Înlocuiește cu cheia ta actuală
            ChatGPTService chatGPTService = new ChatGPTService(apiKey);

            buttonSend.Click += async (sender, e) =>
            {
                string userMessage = textBoxChat.Text;

                if (string.IsNullOrWhiteSpace(userMessage))
                {
                    MessageBox.Show("Introduceți un mesaj valid.", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Obține informațiile actualizate din GameState
                string diceInfo = $"Zaruri: {string.Join(", ", gameState.Dice)}";
                string userScoreInfo = $"Scor utilizator: {gameState.GetTotalScore(gameState.UserScoreCard)}";
                string aiScoreInfo = $"Scor AI: {gameState.GetTotalScore(gameState.AIScoreCard)}";
                string currentPlayer = $"Jucător curent: {gameState.CurrentPlayer}";
                string rollsRemaining = $"Aruncări rămase: {gameState.RollsRemaining}";

                // Construim mesajul complet
                string context = $"{diceInfo}\n{userScoreInfo}\n{aiScoreInfo}\n{currentPlayer}\n{rollsRemaining}";
                string fullMessage = $"{userMessage}\n{context}";

                // Trimitem mesajul către ChatGPT
                string response = await chatGPTService.GetResponseAsync(fullMessage);

                // Afișăm răspunsul în caseta de răspunsuri
                responseBox.AppendText($"Tu: {userMessage}\r\n");
                responseBox.AppendText($"ChatGPT: {response}\r\n");

                // Curățăm caseta de text
                textBoxChat.Clear();
            };


            // Afișăm formularul "Chat" ca modal (acoperă ce este în spate)
            chatForm.Show();
        }

        private void ButtonRegulament_Click(object sender, EventArgs e)
        {
            // Textul regulamentului
            string regulament =
                "🎲 **Regulament Yahtzee - User vs AI** 🎲\n\n" +
                "🟢 **Obiectivul jocului:**\n" +
                "Scopul este să obții cel mai mare scor completând cele 13 categorii disponibile în tabelul de scor.\n\n" +
                "🟢 **Desfășurarea jocului:**\n" +
                "- Jocul se desfășoară pe ture. Fiecare jucător (User sau AI) are 13 runde.\n" +
                "- La fiecare rundă, ai voie să arunci zarurile de până la 3 ori.\n" +
                "- După ce ai terminat aruncările, trebuie să alegi o categorie pentru a înregistra scorul.\n" +
                "- Categoriile necompletate primesc 0 puncte dacă nu se respectă cerințele.\n\n" +
                "🟢 **Categorii și scoruri:**\n" +
                "- **Ones**: Suma zarurilor care arată 1.\n" +
                "- **Twos**: Suma zarurilor care arată 2.\n" +
                "- **Threes**: Suma zarurilor care arată 3.\n" +
                "- **Fours**: Suma zarurilor care arată 4.\n" +
                "- **Fives**: Suma zarurilor care arată 5.\n" +
                "- **Sixes**: Suma zarurilor care arată 6.\n" +
                "- **Three of a Kind**: Aduni toate zarurile dacă ai cel puțin trei zaruri cu aceeași valoare.\n" +
                "- **Full House**: 25 de puncte dacă ai trei zaruri de un fel și două de alt fel.\n" +
                "- **Small Straight**: 30 de puncte pentru patru zaruri consecutive (ex. 1-2-3-4).\n" +
                "- **Large Straight**: 40 de puncte pentru cinci zaruri consecutive (ex. 2-3-4-5-6).\n" +
                "- **Yahtzee**: 50 de puncte pentru cinci zaruri identice.\n" +
                "- **Chance**: Aduni toate zarurile, indiferent de combinație.\n\n" +
                "🟢 **Dificultăți AI:**\n" +
                "- **Slab (Random):** Alege zarurile și categoriile complet aleatoriu.\n" +
                "- **Mediu (Strategie):** Joacă folosind o strategie predefinită pentru a maximiza scorul.\n\n" +
                "🏆 **Cum câștigi:**\n" +
                "La sfârșitul celor 13 runde, jucătorul cu cel mai mare scor total câștigă.\n\n" +
                "🎲 Distracție plăcută! 🎲";

            // Afișăm regulamentul
            MessageBox.Show(regulament, "Regulament", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonStatistici_Click(object sender, EventArgs e)
        {
            // Creăm o instanță de LeaderboardManager
            string leaderboardFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "leaderboard.csv");
            LeaderboardManager leaderboardManager = new LeaderboardManager(leaderboardFilePath);

            // Obținem statisticile detaliate grupate pe utilizatori
            var groupedStatistics = leaderboardManager.GenerateStatisticsGrouped();

            if (groupedStatistics.Count == 0)
            {
                MessageBox.Show("Nu există date suficiente pentru statistici.", "Statistici și progres", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Creăm o fereastră pentru statistici și progres
            Form statsForm = new Form
            {
                Text = "Statistici și progres",
                Size = new Size(1200, 800), // Dimensiuni mai mari
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true // Asigură că această fereastră este deasupra
            };

            // Folosim SplitContainer pentru a împărți fereastra
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 50, // Lățimea părții din stânga
                IsSplitterFixed = false // Permitem ajustarea manuală
            };

            // Partea stângă - informații textuale
            TextBox statsBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill
            };

            // Construim textul pentru afișare
            string statsText = string.Join(Environment.NewLine + Environment.NewLine, groupedStatistics.Select(kvp =>
                $"{kvp.Key}:\n{string.Join(Environment.NewLine, kvp.Value)}"));

            statsBox.Text = statsText;
            splitContainer.Panel1.Controls.Add(statsBox);

            // Partea dreaptă - graficul
            Chart progressChart = new Chart
            {
                Dock = DockStyle.Fill
            };
            progressChart.ChartAreas.Add(new ChartArea("MainArea"));

            // Paletă de culori pentru utilizatori
            List<Color> colorPalette = new List<Color> { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple, Color.Cyan, Color.Magenta };
            int colorIndex = 0;

            foreach (var userStats in groupedStatistics)
            {
                string userName = userStats.Key;
                Color baseColor = colorPalette[colorIndex % colorPalette.Count]; // Culoarea de bază pentru utilizator
                colorIndex++;

                // Date pentru jocurile Weak
                var weakScores = leaderboardManager.GetPlayerScores(userName).GetValueOrDefault("Weak", new List<int>());
                if (weakScores.Any())
                {
                    Series weakSeries = new Series($"{userName} - Weak")
                    {
                        ChartType = SeriesChartType.Line,
                        Color = ControlPaint.Light(baseColor), // Culoare mai deschisă pentru Weak
                        BorderWidth = 2,
                        BorderDashStyle = ChartDashStyle.Dash // Stil punctat pentru Weak
                    };

                    for (int i = 0; i < weakScores.Count; i++)
                    {
                        weakSeries.Points.AddXY(i + 1, weakScores[i]);
                    }

                    progressChart.Series.Add(weakSeries);
                }

                // Date pentru jocurile Medium
                var mediumScores = leaderboardManager.GetPlayerScores(userName).GetValueOrDefault("Medium", new List<int>());
                if (mediumScores.Any())
                {
                    Series mediumSeries = new Series($"{userName} - Medium")
                    {
                        ChartType = SeriesChartType.Line,
                        Color = baseColor, // Culoarea de bază pentru Medium
                        BorderWidth = 2,
                        BorderDashStyle = ChartDashStyle.Solid // Stil continuu pentru Medium
                    };

                    for (int i = 0; i < mediumScores.Count; i++)
                    {
                        mediumSeries.Points.AddXY(i + 1, mediumScores[i]);
                    }

                    progressChart.Series.Add(mediumSeries);
                }
            }

            // Adăugăm legenda
            progressChart.Legends.Add(new Legend("Legend")
            {
                Docking = Docking.Top
            });

            // Adăugăm graficul în partea dreaptă
            splitContainer.Panel2.Controls.Add(progressChart);

            // Adăugăm SplitContainer în fereastră
            statsForm.Controls.Add(splitContainer);

            // Afișăm fereastra
            statsForm.ShowDialog();
        }
    }
}
