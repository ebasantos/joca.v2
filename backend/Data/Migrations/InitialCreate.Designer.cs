using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace WhatsappChatbot.Api.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20240405174000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.3")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("WhatsappChatbot.Api.Models.Contact", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Name")
                .HasColumnType("text");

            b.Property<string>("Phone")
                .IsRequired()
                .HasColumnType("text");

            b.Property<string>("Status")
                .IsRequired()
                .HasColumnType("text");

            b.Property<DateTime>("UpdatedAt")
                .HasColumnType("timestamp with time zone");

            b.HasKey("Id");

            b.HasIndex("Phone")
                .IsUnique();

            b.ToTable("Contacts");
        });

        modelBuilder.Entity("WhatsappChatbot.Api.Models.Message", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<int>("ContactId")
                .HasColumnType("integer");

            b.Property<string>("Content")
                .IsRequired()
                .HasColumnType("text");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Type")
                .IsRequired()
                .HasColumnType("text");

            b.HasKey("Id");

            b.HasIndex("ContactId");

            b.ToTable("Messages");
        });

        modelBuilder.Entity("WhatsappChatbot.Api.Models.Message", b =>
        {
            b.HasOne("WhatsappChatbot.Api.Models.Contact", "Contact")
                .WithMany("Messages")
                .HasForeignKey("ContactId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("Contact");
        });

        modelBuilder.Entity("WhatsappChatbot.Api.Models.Contact", b =>
        {
            b.Navigation("Messages");
        });
    }
} 