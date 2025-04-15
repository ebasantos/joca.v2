using System.ComponentModel.DataAnnotations;

namespace WhatsappChatbot.Api.Models;

public class Contact
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Company { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
} 