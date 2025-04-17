using Hangfire.Dashboard;

namespace WhatsappChatbot.Api.Services;

/// <summary>
/// This filter allows all connections to the Hangfire dashboard.
/// In production, you should restrict access to authenticated users.
/// </summary>
public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Return true to allow everyone to access the dashboard.
        // In production, you might want to implement a proper authorization check.
        return true;
    }
}