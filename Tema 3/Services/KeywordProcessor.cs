using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextAnalysisApp.Services
{
    public class KeywordProcessor
    {
        // Lista de stopwords (cuvinte comune care nu sunt relevante)
        private static readonly HashSet<string> Stopwords = new HashSet<string>
            {
                "a", "an", "and", "are", "as", "at", "be", "by", "for", "from", "has", "he", "in", "is",
                "it", "its", "of", "on", "that", "the", "to", "was", "were", "will", "with", "this", "these",
                "or", "but", "not", "they", "we", "you", "your", "i", "my", "me", "our", "us"
            };

        // Calculăm scorul RAKE simplificat pentru un text
        public Dictionary<string, double> CalculateRakeScores(string text)
        {
            // Tokenizare pentru fraze (împărțim textul în propoziții)
            var phrases = Regex.Split(text.ToLower(), "[.!?,;:\\t\\-()\\[\\]\\\\\"']\\s*")
                               .Where(p => !string.IsNullOrWhiteSpace(p))
                               .ToList();

            // Împărțim frazele în cuvinte, eliminând stopwords
            var phraseWords = phrases.Select(phrase =>
                Regex.Split(phrase, "\\W+")
                     .Where(word => word.Length > 2 && !Stopwords.Contains(word))
                     .ToList()
            );

            // Calculăm scorurile pentru fiecare cuvânt bazat pe co-ocurențe
            var wordScores = new Dictionary<string, double>();

            foreach (var words in phraseWords)
            {
                foreach (var word in words)
                {
                    if (!wordScores.ContainsKey(word))
                        wordScores[word] = 0;

                    wordScores[word] += words.Count; // Creștem scorul proporțional cu fraza
                }
            }

            // Returnăm scorurile RAKE
            return wordScores;
        }

        // Extragem cuvintele cheie folosind RAKE
        public List<string> ExtractKeywordsUsingRake(string text, int maxKeywords = 5)
        {
            var rakeScores = CalculateRakeScores(text);

            // Sortăm cuvintele după scor și selectăm cele mai importante
            return rakeScores.OrderByDescending(kv => kv.Value)
                             .Take(maxKeywords)
                             .Select(kv => kv.Key)
                             .Distinct(StringComparer.OrdinalIgnoreCase) // Eliminăm redundanțele
                             .ToList();
        }

        // Generăm propoziții unice din cuvintele cheie
        public List<string> GenerateUniqueSentencesFromKeywords(List<string> keywords, string text)
        {
            var sentences = new List<string>();

            // Împărțim textul în propoziții
            var textSentences = Regex.Split(text, "(?<=[.!?])\\s+")
                                      .Where(s => !string.IsNullOrWhiteSpace(s))
                                      .ToList();

            var usedSentences = new HashSet<string>(); // Urmărim propozițiile deja utilizate

            foreach (var keyword in keywords)
            {
                // Găsim prima propoziție care conține cuvântul cheie și nu a fost utilizată
                var matchingSentence = textSentences.FirstOrDefault(sentence =>
                    Regex.IsMatch(sentence, $"\\b{Regex.Escape(keyword)}\\b", RegexOptions.IgnoreCase) &&
                    !usedSentences.Contains(sentence));

                if (matchingSentence != null)
                {
                    usedSentences.Add(matchingSentence); // Marcăm propoziția ca utilizată
                    sentences.Add($"Keyword: {keyword}. Context: {matchingSentence.Trim()}");
                }
                else
                {
                    sentences.Add($"Keyword: {keyword}. Context: No unique match found.");
                }
            }

            return sentences;
        }

        // Generăm propoziții generale folosind cuvintele cheie
        public List<string> GenerateGeneralSentencesFromKeywords(List<string> keywords)
        {
            var sentences = new List<string>();

            // Patternuri generalizate pentru propoziții
            var patterns = new List<string>
                {
                    "{0} is a topic of great interest worldwide.",
                    "Many studies have focused on understanding {0}.",
                    "{0} plays a crucial role in various aspects of life.",
                    "The importance of {0} cannot be overlooked.",
                    "{0} has been widely discussed and analyzed.",
                    "People often wonder about the significance of {0}.",
                    "{0} shapes the way we think about many subjects.",
                    "Experts continue to explore the implications of {0}.",
                    "The concept of {0} is deeply embedded in many areas.",
                    "{0} remains an enduring subject of fascination.",
                    
                    "The study of {0} reveals intriguing patterns.",
                    "{0} has a profound impact on our daily lives.",
                    "Discussions about {0} are often thought-provoking.",
                    "Historical perspectives on {0} provide valuable insights.",
                    "{0} challenges traditional ways of thinking.",
                    "Advancements in {0} are reshaping the world.",
                    "Exploring {0} offers unique perspectives.",
                    "{0} connects various disciplines and ideas.",
                    "{0} inspires creativity and innovation.",
                    "The relationship between {0} and society is complex.",
                    "{0} is essential for understanding modern challenges.",
                    "New discoveries in {0} continue to emerge.",
                    "{0} influences the decisions we make every day.",
                    "The history of {0} is rich and multifaceted.",
                    "{0} has been a subject of debate for centuries.",
                    "Our perception of {0} evolves over time.",
                    "{0} is intertwined with cultural and social norms.",
                    "Research on {0} contributes to scientific advancements.",
                    "{0} holds a unique place in human history.",
                    "The dynamics of {0} are ever-changing.",
                    "{0} raises important ethical questions.",
                    "Public opinion about {0} varies widely.",
                    "Understanding {0} requires interdisciplinary approaches.",
                    "The impact of {0} extends across multiple domains.",
                    "{0} offers opportunities for learning and growth.",
                    "{0} is a cornerstone of many theories and practices.",
                    "{0} continues to shape academic discourse.",
                    "The future of {0} is full of possibilities.",
                    "{0} affects everyone in different ways.",
                    "Analyzing {0} provides valuable perspectives.",
                    "{0} has a rich legacy in various cultures.",
                    "{0} requires both critical thinking and creativity.",
                    "The challenges of {0} demand innovative solutions.",
                    "The evolution of {0} reflects societal changes.",
                    "{0} brings together diverse fields of knowledge.",
                    "{0} highlights the complexity of modern issues.",
                    "Collaborations in {0} lead to groundbreaking results.",
                    "{0} often sparks debates among experts.",
                    "The study of {0} combines theory and practice.",
                    "{0} influences policies and decision-making.",
                    "{0} represents a critical area of inquiry.",
                    "Discoveries in {0} are often transformative.",
                    "The exploration of {0} unveils new possibilities.",
                    "{0} has far-reaching consequences for humanity.",
                    "{0} is central to many philosophical discussions.",
                    "The nuances of {0} require careful consideration.",
                    "{0} challenges existing paradigms and ideas.",
                    "{0} bridges gaps between theory and application.",
                    "Insights into {0} continue to expand.",
                    "The significance of {0} is universally acknowledged.",
                    "The relevance of {0} spans across generations.",
                    "{0} embodies a wide range of interpretations.",
                    "{0} is pivotal to understanding complex phenomena.",
                    "{0} intersects with numerous other topics.",
                    "{0} is a driving force in many innovations.",
                    "The intricacies of {0} are endlessly fascinating.",
                    "{0} raises profound questions about the human experience.",
                    "{0} fosters collaboration and exchange of ideas.",
                    "{0} is at the forefront of many advancements.",
                    "Explorations in {0} reveal surprising connections.",
                    "{0} contributes significantly to societal progress.",
                    "{0} has shaped human thought for millennia.",
                    "The scope of {0} is broad and diverse.",
                    "{0} continues to inspire new generations of thinkers."
                };

            var random = new Random();

            foreach (var keyword in keywords)
            {
                // Selectăm un pattern potrivit bazat pe lungimea cuvântului sau alte criterii
                var suitablePatterns = patterns.Where(p => p.Length > keyword.Length + 20).ToList();
                var pattern = suitablePatterns.Count > 0
                    ? suitablePatterns[random.Next(suitablePatterns.Count)]
                    : patterns[random.Next(patterns.Count)];

                var sentence = string.Format(pattern, keyword);
                sentences.Add(sentence);
            }

            return sentences;
        }
    }
}
