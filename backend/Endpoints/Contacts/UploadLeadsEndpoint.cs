using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WhatsappChatbot.Api.Data;
using WhatsappChatbot.Api.Models;

namespace WhatsappChatbot.Api.Endpoints.Contacts;

public class UploadLeadsEndpoint : EndpointWithoutRequest
{
    private readonly AppDbContext _context;
    private readonly ILogger<UploadLeadsEndpoint> _logger;

    public UploadLeadsEndpoint(AppDbContext context, ILogger<UploadLeadsEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/contacts/upload");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (Files.Count == 0)
            {
                ThrowError("Nenhum arquivo foi enviado");
                return;
            }

            var file = Files[0];
            if (!file.FileName.EndsWith(".xlsx"))
            {
                ThrowError("O arquivo deve ser do tipo .xlsx");
                return;
            }

            using var package = new ExcelPackage(file.OpenReadStream());
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            var contacts = new List<Contact>();

            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, 1].Value?.ToString();
                var phoneNumber = worksheet.Cells[row, 2].Value?.ToString();
                var email = worksheet.Cells[row, 3].Value?.ToString();
                var company = worksheet.Cells[row, 4].Value?.ToString();
                var notes = worksheet.Cells[row, 5].Value?.ToString();

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phoneNumber))
                {
                    continue;
                }

                contacts.Add(new Contact
                {
                    Name = name,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Company = company,
                    Notes = notes
                });
            }

            await _context.Contacts.AddRangeAsync(contacts, ct);
            await _context.SaveChangesAsync(ct);

            await SendOkAsync(new { message = $"{contacts.Count} contatos importados com sucesso" }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar arquivo de contatos");
            ThrowError("Erro ao processar arquivo de contatos");
        }
    }
} 