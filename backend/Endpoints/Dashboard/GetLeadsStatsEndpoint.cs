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
            _logger.LogInformation("Fetching lead statistics");

            // Count total leads
            var total = await _context.Contacts.CountAsync(ct);

            // For demo purposes, we'll simulate different statuses
            // In a real system, you would have a Status field on your Contact model
            var processed = (int)(total * 0.7); // Assume 70% processed 
            var failed = (int)(total * 0.1);    // Assume 10% failed
            var pending = total - processed - failed; // The rest are pending

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