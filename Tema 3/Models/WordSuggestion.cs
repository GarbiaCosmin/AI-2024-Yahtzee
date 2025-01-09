namespace TextAnalysisApp.Models
{
    public class WordSuggestion
    {
        public string Word { get; set; } // Sinonim sau termen conex
        public int Score { get; set; } // Scorul relevanței (dacă este furnizat de API)
    }
}
