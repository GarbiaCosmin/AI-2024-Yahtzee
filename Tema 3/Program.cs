using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TextAnalysisApp.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Alege metoda de citire a textului:");
        Console.WriteLine("1 - Introdu textul manual");
        Console.WriteLine("2 - Citește textul dintr-un fișier");
        Console.Write("Introdu opțiunea ta (1 sau 2): ");

        string option = Console.ReadLine();
        string text = string.Empty;

        if (option == "1")
        {
            Console.WriteLine("Introdu textul dorit:");
            text = Console.ReadLine();
        }
        else if (option == "2")
        {
            Console.Write("Introdu calea către fișier: ");
            string filePath = Console.ReadLine();

            if (File.Exists(filePath))
            {
                text = File.ReadAllText(filePath);
                Console.WriteLine("Textul din fișier a fost citit cu succes!");
            }
            else
            {
                Console.WriteLine("Fișierul nu există. Verifică calea și încearcă din nou.");
                return;
            }
        }
        else
        {
            Console.WriteLine("Opțiune invalidă.");
            return;
        }

        Console.WriteLine("\nTextul introdus este:");
        Console.WriteLine(text);

        // Detectăm limba textului
        var detectedLanguage = LanguageUtils.DetectLanguage(text);
        Console.WriteLine($"\nLimba detectată: {detectedLanguage}");

        // Validăm limba detectată
        if (string.IsNullOrEmpty(detectedLanguage) || detectedLanguage == "unknown")
        {
            Console.WriteLine("Limbă nedetectată. Introdu un text mai lung.");
            return;
        }

        // Analiza stilometrică
        AnalyzeStylometry(text);

        // Generăm textul alternativ
        Console.WriteLine("\nGenerăm o versiune alternativă a textului...");
        var textProcessor = new TextProcessor();
        string alternativeText = await textProcessor.GenerateAlternativeText(text);
        Console.WriteLine("\nTextul alternativ generat:");
        Console.WriteLine(alternativeText);

        // Calculăm procentul de cuvinte modificate
        string originalText = text;
        string modifiedText = alternativeText;
        double percentageModified = CalculateModifiedPercentage(originalText, modifiedText);
        Console.WriteLine($"\nProcent de cuvinte modificate: {percentageModified:F2}%");
    }

    static void AnalyzeStylometry(string text)
    {
        // Eliminăm semnele de punctuație și împărțim textul în cuvinte
        var words = Regex.Split(text.ToLower(), @"\W+").Where(w => w.Length > 0).ToList();

        // Calculăm numărul total de cuvinte și caractere
        int wordCount = words.Count;
        int charCount = text.Count(c => !char.IsWhiteSpace(c));
        int charCountWithSpaces = text.Length;

        Console.WriteLine("\nAnaliză stilometrică:");
        Console.WriteLine($"- Număr total de cuvinte: {wordCount}");
        Console.WriteLine($"- Număr total de caractere (fără spații): {charCount}");
        Console.WriteLine($"- Număr total de caractere (cu spații): {charCountWithSpaces}");

        // Frecvența cuvintelor
        var wordFrequency = words.GroupBy(w => w)
                                 .ToDictionary(g => g.Key, g => g.Count());

        Console.WriteLine("\nFrecvența cuvintelor:");
        foreach (var word in wordFrequency.OrderByDescending(w => w.Value))
        {
            Console.WriteLine($"- {word.Key}: {word.Value}");
        }



        Console.WriteLine("\nExtragem cuvintele cheie folosind RAKE...");
        var keywordProcessor = new KeywordProcessor();
        var keywords = keywordProcessor.ExtractKeywordsUsingRake(text, maxKeywords: 5);
        Console.WriteLine("\nCuvinte cheie găsite:");
        Console.WriteLine(string.Join(", ", keywords));

   Console.WriteLine("\nGenerăm propoziții generale folosind cuvintele cheie...");
var generalSentences = keywordProcessor.GenerateGeneralSentencesFromKeywords(keywords);
Console.WriteLine("\nPropoziții generate din cuvintele cheie:");
foreach (var sentence in generalSentences)
{
    Console.WriteLine(sentence);
}


    }

    static double CalculateModifiedPercentage(string originalText, string modifiedText)
    {
        // Extragem cuvintele din textele originale și modificate
        var originalWords = Regex.Matches(originalText, @"\b\w+\b").Cast<Match>().Select(m => m.Value).ToList();
        var modifiedWords = Regex.Matches(modifiedText, @"\b\w+\b").Cast<Match>().Select(m => m.Value).ToList();

        // Verificăm dacă există cuvinte valide
        if (originalWords.Count == 0)
        {
            Console.WriteLine("Eroare: Textul original nu conține cuvinte valide.");
            return 0.0;
        }

        // Calculăm lungimea minimă dintre cele două liste
        int minLength = Math.Min(originalWords.Count, modifiedWords.Count);

        // Numărăm modificările efective
        int modifiedCount = 0;
        for (int i = 0; i < minLength; i++)
        {
            if (!originalWords[i].Equals(modifiedWords[i], StringComparison.OrdinalIgnoreCase))
            {
                modifiedCount++;
            }
        }

        // Calculăm procentul modificat
        double percentageModified = (double)modifiedCount / originalWords.Count * 100;

        return percentageModified;
    }

}