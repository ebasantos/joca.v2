using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WhatsappChatbot.Api.Data;
using WhatsappChatbot.Api.Models;

namespace WhatsappChatbot.Api.Endpoints.Dashboard;

public class LeadsStatsResponse
{
    public int Total { get; set; }
    public int Processed { get; set; }
    public int Pending { get; set; }
    public int Failed { get; set; }
}

public class GetLeadsStatsEndpoint : EndpointWithoutRequest<LeadsStatsResponse>
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetLeadsStatsEndpoint> _logger;

    public GetLeadsStatsEndpoint(AppDbContext context, ILogger<GetLeadsStatsEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/leads/stats");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Fetching lead statistics from database");

            // Get real counts from the database using the Status field
            var total = await _context.Contacts.CountAsync(ct);
            var processed = await _context.Contacts.CountAsync(c => c.Status == "processed", ct);
            var failed = await _context.Contacts.CountAsync(c => c.Status == "failed", ct);
            var pending = await _context.Contacts.CountAsync(c => c.Status == "pending", ct);

            // Additional validation to ensure the counts add up
            if (processed + failed + pending != total)
            {
                // This could happen if there are contacts with unexpected status values
                _logger.LogWarning("Stats validation failed: processed ({processed}) + failed ({failed}) + pending ({pending}) != total ({total})",
                    processed, failed, pending, total);
            }

            var response = new LeadsStatsResponse
            {
                Total = total,
                Processed = processed,
                Pending = pending,
                Failed = failed
            };

            await SendOkAsync(response, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lead statistics");
            ThrowError("Error fetching lead statistics");
        }
    }
}