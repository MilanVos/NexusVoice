namespace NexusVoice.Models;

public enum Speaker { Dutch, German }

public class ConversationMessage
{
    public Speaker Speaker { get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public bool IsFromDutch => Speaker == Speaker.Dutch;
    public string SpeakerLabel => Speaker == Speaker.Dutch ? "🇳🇱 Jij" : "🇩🇪 Duitser";
    public string TranslationFlag => Speaker == Speaker.Dutch ? "🇩🇪" : "🇳🇱";
    public string TimeText => Timestamp.ToString("HH:mm");
}
