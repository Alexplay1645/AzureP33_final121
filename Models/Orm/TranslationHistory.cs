using System;

namespace AzureP33.Models
{
    public class TranslationHistory
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public string OriginalText { get; set; } = null!;
        public string OriginalLanguage { get; set; } = null!;
        public string TranslatedText { get; set; } = null!;
        public string TranslatedLanguage { get; set; } = null!;
        public string? OriginalTransliteration { get; set; }
        public string? OriginalScript { get; set; }
        public string? TranslatedTransliteration { get; set; }
        public string? TranslatedScript { get; set; }
    }
}
