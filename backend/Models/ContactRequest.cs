using System.ComponentModel.DataAnnotations;

namespace WhatsappChatbot.Api.Models;

public class ContactRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O número de telefone é obrigatório")]
    [Phone(ErrorMessage = "O número de telefone não é válido")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "O email não é válido")]
    public string? Email { get; set; }
    
    public string? Company { get; set; }
    
    public string? Notes { get; set; }
} 