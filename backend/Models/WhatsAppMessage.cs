namespace WhatsappChatbot.Api.Models;

public class WhatsAppMessage
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = "text"; // text, image, video, audio, etc.
    public string? MediaUrl { get; set; }
} 