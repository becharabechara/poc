using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;
using System.Text.Json;

namespace SmartNotes.Infrastructure.Data.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        // Table configuration
        builder.ToTable("Notes", "smartnotes");

        // Primary key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // We generate GUIDs in the domain

        // Title configuration - use backing field
        builder.Property(x => x.Title)
            .HasField("_title")
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        // Content configuration - use backing field
        builder.Property(x => x.Content)
            .HasField("_content")
            .HasColumnName("content")
            .HasMaxLength(10000)
            .IsRequired();

        // Tags configuration - use backing field and store as JSON
        builder.Property(x => x.Tags)
            .HasField("_tags")
            .HasColumnName("tags")
            .HasColumnType("jsonb")
            .HasConversion(
                tags => JsonSerializer.Serialize(tags!.Select(t => t.Value), (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<string[]>(json, (JsonSerializerOptions?)null)!
                    .Select(value => new Tag(value))
                    .ToList()
                    .AsReadOnly(),
                new ValueComparer<IReadOnlyCollection<Tag>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList().AsReadOnly()
                ));

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Indexes for performance
        builder.HasIndex(x => x.Title)
            .HasDatabaseName("ix_notes_title");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_notes_created_at");

        builder.HasIndex(x => x.UpdatedAt)
            .HasDatabaseName("ix_notes_updated_at");

        // GIN index for JSONB tags column (PostgreSQL specific)
        builder.HasIndex(x => x.Tags)
            .HasMethod("gin")
            .HasDatabaseName("ix_notes_tags_gin");
    }
}