using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WhatsappChatbot.Api.Models;

public class Message
{
    [Key]
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int ContactId { get; set; }

    [ForeignKey("ContactId")]
    public Contact Contact { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
} 