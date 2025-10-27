using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartNotes.Application.Ports;
using SmartNotes.Infrastructure.Data;
using SmartNotes.Infrastructure.Repositories;

namespace SmartNotes.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<SmartNotesDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(SmartNotesDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            // Configure for different environments
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Add repositories
        services.AddScoped<INoteRepository, NoteRepository>();

        // Add health checks for database
        services.AddHealthChecks()
            .AddDbContextCheck<SmartNotesDbContext>("database");

        return services;
    }

    public static IServiceCollection AddInfrastructureForTesting(
        this IServiceCollection services,
        string connectionString)
    {
        // Add DbContext for testing with in-memory database
        services.AddDbContext<SmartNotesDbContext>(options =>
        {
            options.UseInMemoryDatabase(connectionString);
            
            // Enable detailed logging for tests
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        // Add repositories
        services.AddScoped<INoteRepository, NoteRepository>();

        return services;
    }
}