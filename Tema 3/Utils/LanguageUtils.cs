using LanguageDetection;

internal class LanguageUtils
{
    public static string DetectLanguage(string text)
    {
        try
        {
            var detector = new LanguageDetector();
            detector.AddAllLanguages();

            // Detectăm limba textului
            var language = detector.Detect(text);

            // Verificăm dacă detectarea a fost de succes
            return !string.IsNullOrEmpty(language) ? language : "Limbă necunoscută";
        }
        catch (Exception ex)
        {
            // În cazul unei erori, returnăm un mesaj corespunzător
            Console.WriteLine($"Eroare la detectarea limbii: {ex.Message}");
            return "Eroare";
        }
    }
}
