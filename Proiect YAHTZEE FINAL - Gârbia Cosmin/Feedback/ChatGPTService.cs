using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YahtzeeProject.Feedback
{
    public class ChatGPTService
    {
        private readonly string apiKey;

        public ChatGPTService(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<string> GetResponseAsync(string prompt, int[] diceConfig = null, List<string> availableCategories = null)
        {
            try
            {
                // Construim mesajele
                var messages = new List<object>
        {
            new { role = "system", content = "Ești un asistent pentru jocul Yahtzee. Poți explica reguli, oferi strategii sau face recomandări." },
            new { role = "user", content = prompt }
        };

                if (diceConfig != null && availableCategories != null)
                {
                    string diceInfo = $"Configurarea zarurilor: {string.Join(", ", diceConfig)}";
                    string categoriesInfo = $"Categoriile disponibile: {string.Join(", ", availableCategories)}";
                    messages.Add(new { role = "user", content = $"{diceInfo}\n{categoriesInfo}" });
                }

                var requestBody = new
                {
                    model = "gpt-4o-mini", // Specificăm modelul
                    messages = messages,
                    temperature = 0.7
                };

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                    // Logăm cererea
                    Console.WriteLine($"Request: {JsonSerializer.Serialize(requestBody)}");

                    var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Logăm răspunsul
                    Console.WriteLine($"Response: {response.StatusCode} | {responseString}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseObject = JsonSerializer.Deserialize<JsonDocument>(responseString);
                        return responseObject.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                    }
                    else
                    {
                        return $"Eroare la conectarea cu API-ul: {response.StatusCode} | {responseString}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Eroare la conectarea cu API-ul: {ex.Message}";
            }
        }

    }
}
