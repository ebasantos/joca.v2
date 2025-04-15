using System.Text;
using System.Text.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WhatsappChatbot.Api.Models;
using WhatsappChatbot.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;

namespace WhatsappChatbot.Api.Services;

public interface IChatbotService
{
    Task<string> ProcessMessageAsync(string message, Contact contact, CancellationToken ct = default);
}

public class ChatbotService : IChatbotService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly ILogger<ChatbotService> _logger;

    public ChatbotService(AppDbContext context, IConfiguration configuration, ILogger<ChatbotService> logger)
    {
        _context = context;
        _httpClient = new HttpClient();
        _apiKey = configuration["DeepSeek:ApiKey"] ?? throw new InvalidOperationException("DeepSeek API Key não configurada");
        _apiUrl = configuration["DeepSeek:ApiUrl"] ?? throw new InvalidOperationException("DeepSeek API URL não configurada");
        _logger = logger;
    }

    public async Task<string> ProcessMessageAsync(string message, Contact contact, CancellationToken ct = default)
    {
        try
        {
            var response = await GenerateResponseAsync(message, contact, ct);

            var outboundMessage = new Models.Message
            {
                Content = response,
                Direction = "outbound",
                Status = "sent",
                ContactId = contact.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(outboundMessage);
            await _context.SaveChangesAsync(ct);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem");
            return "Desculpe, ocorreu um erro ao processar sua mensagem. Por favor, tente novamente mais tarde.";
        }
    }

    private async Task<string> GenerateResponseAsync(string message, Contact contact, CancellationToken ct)
    {
        var systemPrompt = "Você é um assistente virtual profissional e amigável. " +
                          "Mantenha suas respostas concisas e relevantes. " +
                          "Use um tom cordial e profissional.";

        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"Nome do contato: {contact.Name}\nMensagem: {message}" }
            },
            temperature = 0.7,
            max_tokens = 500
        };

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsJsonAsync(_apiUrl, requestBody, ct);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadFromJsonAsync<DeepSeekResponse>(cancellationToken: ct);
        return responseContent?.Choices?[0]?.Message?.Content ?? "Desculpe, não consegui gerar uma resposta.";
    }
}

public class DeepSeekResponse
{
    public Choice[]? Choices { get; set; }
}

public class Choice
{
    public DeepSeekMessage? Message { get; set; }
}

public class DeepSeekMessage
{
    public string? Content { get; set; }
} 