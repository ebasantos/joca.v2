namespace WhatsappChatbot.Api.Endpoints.Contacts;

public class CreateContactRequest
{
    public string Phone { get; set; } = string.Empty;
    public string? Name { get; set; }
} 