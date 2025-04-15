using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WhatsappChatbot.Api.Data;
using WhatsappChatbot.Api.Models;

namespace WhatsappChatbot.Api.Endpoints.Contacts;

public class CreateContactEndpoint : Endpoint<ContactRequest>
{
    private readonly AppDbContext _context;
    private readonly ILogger<CreateContactEndpoint> _logger;

    public CreateContactEndpoint(AppDbContext context, ILogger<CreateContactEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/contacts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ContactRequest req, CancellationToken ct)
    {
        try
        {
            // Verificar se já existe um contato com o mesmo número de telefone
            var existingContact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.PhoneNumber == req.PhoneNumber, ct);

            if (existingContact != null)
            {
                ThrowError("Já existe um contato com este número de telefone");
                return;
            }

            var contact = new Contact
            {
                Name = req.Name,
                PhoneNumber = req.PhoneNumber,
                Email = req.Email,
                Company = req.Company,
                Notes = req.Notes
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync(ct);

            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar contato");
            ThrowError("Erro ao criar contato");
        }
    }
} 