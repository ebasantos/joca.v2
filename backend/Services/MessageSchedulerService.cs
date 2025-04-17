using Hangfire;
using WhatsappChatbot.Api.Models;
using WhatsappChatbot.Api.Data;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.EntityFrameworkCore;

namespace WhatsappChatbot.Api.Services;

public class MessageSchedulerService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageSchedulerService> _logger;

    public MessageSchedulerService(
        AppDbContext context,
        IConfiguration configuration,
        ILogger<MessageSchedulerService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Schedule a WhatsApp message to be sent immediately
    /// </summary>
    public void ScheduleMessage(int contactId, string messageContent)
    {
        // Fire-and-forget job
        BackgroundJob.Enqueue(() => SendWhatsAppMessage(contactId, messageContent));
    }

    /// <summary>
    /// Schedule a WhatsApp message to be sent at a specific time
    /// </summary>
    public void ScheduleMessage(int contactId, string messageContent, DateTime sendAt)
    {
        // Delayed job
        BackgroundJob.Schedule(() => SendWhatsAppMessage(contactId, messageContent), sendAt - DateTime.Now);
    }

    /// <summary>
    /// Schedule welcome messages for a list of contact IDs
    /// </summary>
    public void ScheduleWelcomeMessages(List<int> contactIds)
    {
        // Get welcome message template from configuration
        string welcomeTemplate = _configuration["Messages:WelcomeTemplate"] ??
            "Olá";

        foreach (var contactId in contactIds)
        {
            // Delay each message by a few seconds to avoid rate limiting
            var delay = TimeSpan.FromSeconds(contactIds.IndexOf(contactId) * 5);

            // Schedule the job with the calculated delay
            BackgroundJob.Schedule(
                () => SendWelcomeMessage(contactId, welcomeTemplate),
                delay);
        }
    }

    /// <summary>
    /// Send a personalized welcome message to a contact
    /// </summary>
    public async Task SendWelcomeMessage(int contactId, string messageTemplate)
    {
        try
        {
            // Get contact details
            var contact = await _context.Contacts.FindAsync(contactId);
            if (contact == null)
            {
                _logger.LogWarning("Contact with ID {ContactId} not found for welcome message", contactId);
                return;
            }

            // Personalize message with contact's name
            string personalized = messageTemplate.Replace("{{Name}}", contact.Name);

            // Send the message
            await SendWhatsAppMessage(contactId, personalized);

            // Update contact status to processed
            contact.Status = "processed";
            contact.UpdatedAt = DateTime.UtcNow;
            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contact {ContactId} status updated to processed after welcome message", contactId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending welcome message to contact {ContactId}", contactId);
            throw; // Rethrow for Hangfire to retry
        }
    }

    /// <summary>
    /// Method that actually sends the WhatsApp message (will be called by Hangfire)
    /// </summary>
    public async Task SendWhatsAppMessage(int contactId, string messageContent)
    {
        try
        {
            // Get contact from database
            var contact = await _context.Contacts.FindAsync(contactId);
            if (contact == null)
            {
                _logger.LogWarning("Contact with ID {ContactId} not found", contactId);
                return;
            }

            // Initialize Twilio client
            string accountSid = _configuration["Twilio:AccountSid"] ??
                throw new InvalidOperationException("Twilio Account SID not configured");
            string authToken = _configuration["Twilio:AuthToken"] ??
                throw new InvalidOperationException("Twilio Auth Token not configured");
            string fromNumber = _configuration["Twilio:WhatsAppNumber"] ??
                throw new InvalidOperationException("Twilio WhatsApp Number not configured");

            TwilioClient.Init(accountSid, authToken);

            // Send message via Twilio
            var message = MessageResource.Create(
                from: new PhoneNumber($"whatsapp:{fromNumber}"),
                to: new PhoneNumber($"whatsapp:{contact.PhoneNumber}"),
                body: messageContent
            );

            _logger.LogInformation("WhatsApp message sent to {PhoneNumber}, Twilio SID: {MessageSid}",
                contact.PhoneNumber, message.Sid);

            // Store the message in database
            var outboundMessage = new Models.Message
            {
                Content = messageContent,
                Direction = "outbound",
                Status = "sent",
                ContactId = contact.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(outboundMessage);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message to contact {ContactId}", contactId);
            throw; // Rethrow for Hangfire to retry
        }
    }

    /// <summary>
    /// Schedule a recurring job to check for inactive conversations
    /// </summary>
    public void ScheduleInactiveConversationsCheck()
    {
        // Run this job every day at 8:00 AM
        RecurringJob.AddOrUpdate(
            "check-inactive-conversations",
            () => MarkInactiveConversationsAsFailed(),
            Cron.Daily(8, 0));

        _logger.LogInformation("Scheduled daily job to check for inactive conversations");
    }

    /// <summary>
    /// Find conversations with no response for more than 24 hours and mark them as failed
    /// </summary>
    public async Task MarkInactiveConversationsAsFailed()
    {
        try
        {
            _logger.LogInformation("Checking for inactive conversations");

            // Calculate the cutoff time (24 hours ago)
            var cutoffTime = DateTime.UtcNow.AddHours(-24);

            // Find all contacts who have received outbound messages
            var contactsWithPendingResponses = await _context.Contacts
                .Where(c => c.Messages.Any(m => m.Direction == "outbound"))
                .Select(c => new
                {
                    Contact = c,
                    LastOutboundMessage = c.Messages
                        .Where(m => m.Direction == "outbound")
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault(),
                    LastInboundMessage = c.Messages
                        .Where(m => m.Direction == "inbound")
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault()
                })
                .ToListAsync();

            int failedCount = 0;

            foreach (var item in contactsWithPendingResponses)
            {
                // Skip if there's no outbound message
                if (item.LastOutboundMessage == null)
                    continue;

                bool shouldMarkAsFailed = false;

                // If there's no inbound message at all, check if outbound message is older than 24 hours
                if (item.LastInboundMessage == null)
                {
                    shouldMarkAsFailed = item.LastOutboundMessage.CreatedAt < cutoffTime;
                }
                // If there is an inbound message, check if the last outbound is more recent and older than 24 hours
                else if (item.LastOutboundMessage.CreatedAt > item.LastInboundMessage.CreatedAt)
                {
                    shouldMarkAsFailed = item.LastOutboundMessage.CreatedAt < cutoffTime;
                }

                if (shouldMarkAsFailed)
                {
                    // Create a status update message
                    var statusMessage = new Models.Message
                    {
                        Content = "Conversation marked as failed due to inactivity",
                        Direction = "system",
                        Status = "failed",
                        ContactId = item.Contact.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Messages.Add(statusMessage);

                    // Update the contact status
                    item.Contact.Status = "failed";
                    item.Contact.UpdatedAt = DateTime.UtcNow;
                    _context.Contacts.Update(item.Contact);

                    failedCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Marked {count} conversations as failed due to inactivity", failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking inactive conversations as failed");
            throw;
        }
    }
}