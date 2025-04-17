using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WhatsappChatbot.Api.Data;
using WhatsappChatbot.Api.Models;

namespace WhatsappChatbot.Api.Endpoints.Dashboard;

public class RecentLeadResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "processed", "pending", or "failed"
    public DateTime ImportDate { get; set; }
}

public class GetRecentLeadsEndpoint : EndpointWithoutRequest<List<RecentLeadResponse>>
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetRecentLeadsEndpoint> _logger;

    public GetRecentLeadsEndpoint(AppDbContext context, ILogger<GetRecentLeadsEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/leads/recent");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching recent leads from database");

            // Get the 10 most recent contacts with their actual status
            var recentContacts = await _context.Contacts
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .ToListAsync(ct);

            // Convert to response format with actual status
            var response = recentContacts.Select(contact => new RecentLeadResponse
            {
                Id = contact.Id,
                Name = contact.Name,
                Phone = contact.PhoneNumber,
                // Use the actual status from the database
                Status = contact.Status,
                ImportDate = contact.CreatedAt
            }).ToList();

            await SendOkAsync(response, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent leads");
            ThrowError("Error fetching recent leads");
        }
    }
}