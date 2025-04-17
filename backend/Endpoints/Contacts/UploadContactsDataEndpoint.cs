using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WhatsappChatbot.Api.Data;
using WhatsappChatbot.Api.Models;
using WhatsappChatbot.Api.Services;

namespace WhatsappChatbot.Api.Endpoints.Contacts;

public class ContactDataRequest
{
    public List<ContactData> Contacts { get; set; } = new();
}

public class ContactData
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
}

public class UploadContactsDataEndpoint : Endpoint<ContactDataRequest>
{
    private readonly AppDbContext _context;
    private readonly ILogger<UploadContactsDataEndpoint> _logger;
    private readonly MessageSchedulerService _messageScheduler;

    public UploadContactsDataEndpoint(
        AppDbContext context,
        ILogger<UploadContactsDataEndpoint> logger,
        MessageSchedulerService messageScheduler)
    {
        _context = context;
        _logger = logger;
        _messageScheduler = messageScheduler;
    }

    public override void Configure()
    {
        Post("/api/leads/upload");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ContactDataRequest req, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Received upload request with {count} contacts", req.Contacts?.Count ?? 0);

            if (req.Contacts == null || req.Contacts.Count == 0)
            {
                _logger.LogWarning("No contacts found in request");
                ThrowError("Nenhum contato foi enviado");
                return;
            }

            int newCount = 0;
            int updatedCount = 0;
            List<int> newContactIds = new List<int>();

            // Normalize all phone numbers in the request
            foreach (var contact in req.Contacts)
            {
                if (!string.IsNullOrEmpty(contact.PhoneNumber))
                {
                    // Normalize by removing spaces, hyphens, and ensuring format
                    contact.PhoneNumber = NormalizePhoneNumber(contact.PhoneNumber);
                    _logger.LogInformation("Normalized phone number: {PhoneNumber}", contact.PhoneNumber);
                }
                else
                {
                    _logger.LogWarning("Empty phone number for contact: {Name}", contact.Name);
                }
            }

            // Use a transaction to ensure data consistency
            using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // Process each contact individually to avoid batch update issues
                foreach (var contactData in req.Contacts)
                {
                    if (string.IsNullOrEmpty(contactData.Name) || string.IsNullOrEmpty(contactData.PhoneNumber))
                    {
                        continue;
                    }

                    // Check if the contact already exists using direct query
                    var normalizedPhoneNumber = contactData.PhoneNumber.Trim();
                    _logger.LogInformation("Looking for existing contact with phone number: '{PhoneNumber}'", normalizedPhoneNumber);

                    var existingContact = await _context.Contacts
                        .FirstOrDefaultAsync(c => c.PhoneNumber == normalizedPhoneNumber, ct);

                    if (existingContact != null)
                    {
                        _logger.LogInformation("Found existing contact: ID={Id}, Name={Name}, PhoneNumber={PhoneNumber}",
                            existingContact.Id, existingContact.Name, existingContact.PhoneNumber);

                        // Update existing contact
                        existingContact.Name = contactData.Name;
                        existingContact.Email = contactData.Email;
                        existingContact.Company = contactData.Company;
                        existingContact.Notes = contactData.Notes;

                        _context.Contacts.Update(existingContact);
                        updatedCount++;
                    }
                    else
                    {
                        _logger.LogInformation("No existing contact found for phone number: '{PhoneNumber}', creating new contact", normalizedPhoneNumber);

                        // Create new contact
                        var newContact = new Contact
                        {
                            Name = contactData.Name,
                            PhoneNumber = contactData.PhoneNumber,
                            Email = contactData.Email,
                            Company = contactData.Company,
                            Notes = contactData.Notes
                        };

                        await _context.Contacts.AddAsync(newContact, ct);
                        await _context.SaveChangesAsync(ct); // Save to get the ID

                        newContactIds.Add(newContact.Id); // Store the new contact ID
                        newCount++;
                    }

                    // Save changes after each contact to avoid batch conflicts
                    await _context.SaveChangesAsync(ct);
                }

                await transaction.CommitAsync(ct);

                // If we have new contacts, schedule welcome messages
                if (newContactIds.Count > 0)
                {
                    _logger.LogInformation("Scheduling welcome messages for {count} new contacts", newContactIds.Count);
                    _messageScheduler.ScheduleWelcomeMessages(newContactIds);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error processing contacts during transaction");
                throw;
            }

            await SendOkAsync(new
            {
                count = newCount + updatedCount,
                newContacts = newCount,
                updatedContacts = updatedCount,
                welcomeMessagesScheduled = newContactIds.Count
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar dados de contatos");
            ThrowError("Erro ao processar dados de contatos: " + ex.Message);
        }
    }

    // Helper method to normalize phone numbers
    private string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return phoneNumber;

        _logger.LogDebug("Normalizing phone number. Original: {Original}, Type: {Type}",
            phoneNumber, phoneNumber.GetType().FullName);

        // Remove any non-digit characters
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        _logger.LogDebug("Phone number after normalization: {Normalized}", digitsOnly);

        // Ensure consistent format - you may need to adjust this based on your requirements
        // This is a simple implementation - consider using a proper phone number library for production
        return digitsOnly;
    }
}