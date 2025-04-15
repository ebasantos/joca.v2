using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WhatsappChatbot.Api.Data;
using WhatsappChatbot.Api.Models;

namespace WhatsappChatbot.Api.Endpoints.Contacts;

public class GetContactsEndpoint : EndpointWithoutRequest
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetContactsEndpoint> _logger;

    public GetContactsEndpoint(AppDbContext context, ILogger<GetContactsEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/contacts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var contacts = await _context.Contacts
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

            await SendOkAsync(contacts, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar contatos");
            await SendErrorsAsync(500, ct);
        }
    }
} 