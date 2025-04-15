using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WhatsappChatbot.Api.Data;
using WhatsappChatbot.Api.Models;
using WhatsappChatbot.Api.Services;
using Twilio.TwiML;
using Twilio.TwiML.Messaging;

namespace WhatsappChatbot.Api.Endpoints.WhatsApp;

public class WebhookEndpoint : EndpointWithoutRequest
{
    private readonly AppDbContext _context;
    private readonly IChatbotService _chatbotService;
    private readonly ILogger<WebhookEndpoint> _logger;

    public WebhookEndpoint(AppDbContext context, IChatbotService chatbotService, ILogger<WebhookEndpoint> logger)
    {
        _context = context;
        _chatbotService = chatbotService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/whatsapp/webhook");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var form = await HttpContext.Request.ReadFormAsync(ct);
            var messageBody = form["Body"].ToString();
            var from = form["From"].ToString();

            // Extrair o número do telefone do formato WhatsApp (ex: whatsapp:+5511999999999)
            var phoneNumber = from.Replace("whatsapp:", "");

            // Buscar ou criar o contato
            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, ct);

            if (contact == null)
            {
                contact = new Contact
                {
                    Name = phoneNumber, // Usar o número como nome temporário
                    PhoneNumber = phoneNumber
                };
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync(ct);
            }

            // Salvar a mensagem recebida
            var inboundMessage = new Models.Message
            {
                Content = messageBody,
                Direction = "inbound",
                Status = "received",
                ContactId = contact.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Messages.Add(inboundMessage);

            // Processar a mensagem e gerar resposta
            var response = await _chatbotService.ProcessMessageAsync(messageBody, contact, ct);

            // Criar resposta TwiML
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message(response);

            await SendStringAsync(messagingResponse.ToString(), contentType: "application/xml", cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook do WhatsApp");
            ThrowError("Erro ao processar mensagem");
        }
    }
}

public class WhatsAppWebhook
{
    public List<WebhookEntry>? Entry { get; set; }
}

public class WebhookEntry
{
    public List<WebhookChange>? Changes { get; set; }
}

public class WebhookChange
{
    public WebhookValue? Value { get; set; }
}

public class WebhookValue
{
    public List<WebhookMessage>? Messages { get; set; }
}

public class WebhookMessage
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string? Type { get; set; }
    public WebhookText? Text { get; set; }
}

public class WebhookText
{
    public string? Body { get; set; }
} 