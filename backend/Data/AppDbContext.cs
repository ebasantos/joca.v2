using Microsoft.EntityFrameworkCore;
using WhatsappChatbot.Api.Models;

namespace WhatsappChatbot.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>()
            .HasIndex(c => c.PhoneNumber)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Seed do usuário admin
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@admin.com",
                Password = "admin123", // Em produção, isso deve ser criptografado
                Name = "Administrador",
                CreatedAt = DateTime.UtcNow
            }
        );

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Contact)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ContactId);
    }
} 