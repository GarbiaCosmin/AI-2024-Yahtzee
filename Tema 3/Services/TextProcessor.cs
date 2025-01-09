using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TextAnalysisApp.Models;

namespace TextAnalysisApp.Services
{
    public class TextProcessor
    {
        private readonly HttpClient _httpClient;

        public TextProcessor()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GenerateAlternativeText(string text)
        {
            var random = new Random();

            // Tokenizare pentru cuvinte, spații și punctuație
            var tokens = Regex.Matches(text, @"\w+|[^\w\s]|[\s]+")
                              .Cast<Match>()
                              .Select(m => m.Value)
                              .ToList();

            var modifiedTokens = new List<string>();
            string previousWord = string.Empty;

            foreach (var token in tokens)
            {
                if (Regex.IsMatch(token, @"^\w+$")) // Dacă este un cuvânt
                {
                    if (random.NextDouble() <= 0.4) // 40% șansă să înlocuim cuvântul
                    {
                        string antonymApiUrl = $"https://api.datamuse.com/words?rel_ant={token}";
                        try
                        {
                            var antonyms = await _httpClient.GetFromJsonAsync<List<WordSuggestion>>(antonymApiUrl);
                            if (antonyms != null && antonyms.Count > 0)
                            {
                                // Filtrăm antonimele
                                var validAntonyms = antonyms
                                    .Where(s => !string.IsNullOrWhiteSpace(s.Word) &&
                                                s.Word.Split(' ').Length == 1 && // Doar un singur cuvânt
                                                !s.Word.Equals(token, StringComparison.OrdinalIgnoreCase) && // Diferit de original
                                                Math.Abs(s.Word.Length - token.Length) <= 3) // Lungime similară
                                    .Select(s => s.Word)
                                    .ToList();

                                if (validAntonyms.Any())
                                {
                                    // Folosim forma negată a antonimului
                                    var negatedAntonym = $"not {validAntonyms.First()}";
                                    modifiedTokens.Add(negatedAntonym);
                                    previousWord = negatedAntonym;
                                    continue;
                                }
                            }
                        }
                        catch
                        {
                            // În caz de eroare, trecem la sinonime
                        }

                        // Dacă nu am găsit antonime sau a apărut o eroare, încercăm sinonime
                        string synonymApiUrl = $"https://api.datamuse.com/words?rel_syn={token}";
                        try
                        {
                            var synonyms = await _httpClient.GetFromJsonAsync<List<WordSuggestion>>(synonymApiUrl);
                            if (synonyms != null && synonyms.Count > 0)
                            {
                                var validSynonyms = synonyms
                                    .Where(s => !string.IsNullOrWhiteSpace(s.Word) &&
                                                s.Word.Split(' ').Length == 1 && // Doar un singur cuvânt
                                                !s.Word.Equals(token, StringComparison.OrdinalIgnoreCase) && // Diferit de original
                                                Math.Abs(s.Word.Length - token.Length) <= 3) // Lungime similară
                                    .Select(s => s.Word)
                                    .ToList();

                                var synonym = validSynonyms.OrderByDescending(s => s.Length).FirstOrDefault();
                                modifiedTokens.Add(synonym ?? token);
                                previousWord = synonym ?? token;
                            }
                            else
                            {
                                modifiedTokens.Add(token);
                                previousWord = token;
                            }
                        }
                        catch
                        {
                            modifiedTokens.Add(token);
                            previousWord = token;
                        }
                    }
                    else
                    {
                        modifiedTokens.Add(token);
                        previousWord = token;
                    }
                }
                else if (Regex.IsMatch(token, @"^\s+$")) // Dacă este spațiu
                {
                    modifiedTokens.Add(token);
                }
                else // Dacă este punctuație
                {
                    if (modifiedTokens.Count > 0 && !string.IsNullOrEmpty(previousWord))
                    {
                        // Atașăm punctuația direct după ultimul cuvânt
                        modifiedTokens[modifiedTokens.Count - 1] += token;
                    }
                    else
                    {
                        modifiedTokens.Add(token);
                    }
                }
            }

            // Reconstruim textul
            return string.Join("", modifiedTokens);
        }
    }
}
