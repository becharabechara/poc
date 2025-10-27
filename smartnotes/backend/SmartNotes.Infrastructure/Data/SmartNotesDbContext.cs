using Microsoft.EntityFrameworkCore;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;
using SmartNotes.Infrastructure.Data.Configurations;

namespace SmartNotes.Infrastructure.Data;

public class SmartNotesDbContext : DbContext
{
    public SmartNotesDbContext(DbContextOptions<SmartNotesDbContext> options) : base(options)
    {
    }

    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartNotesDbContext).Assembly);

        // Global query filters and configurations
        ConfigureGlobalSettings(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        // Configure command timeout
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    private static void ConfigureGlobalSettings(ModelBuilder modelBuilder)
    {
        // Configure all string properties to have a maximum length by default
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(string) && p.GetMaxLength() == null))
        {
            property.SetMaxLength(500); // Default max length
        }

        // Configure datetime properties to use UTC
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("timestamp with time zone");
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        UpdateTimestamps();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Note>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                // Use reflection to update UpdatedAt since it's private setter
                var updateMethod = typeof(Note).GetMethod("Update", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (updateMethod != null && entry.Entity.Title != null && entry.Entity.Content != null)
                {
                    var currentTags = entry.Entity.Tags ?? new List<Tag>();
                    updateMethod.Invoke(entry.Entity, new object[] { entry.Entity.Title, entry.Entity.Content, currentTags });
                }
            }
        }
    }
}