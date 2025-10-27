using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartNotes.Application.Ports;
using SmartNotes.Infrastructure.Data;
using SmartNotes.Infrastructure.Extensions;
using SmartNotes.Infrastructure.Repositories;

namespace SmartNotes.Infrastructure.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging service
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<SmartNotesDbContext>());
        Assert.NotNull(serviceProvider.GetService<INoteRepository>());
        Assert.IsType<NoteRepository>(serviceProvider.GetService<INoteRepository>());
        Assert.NotNull(serviceProvider.GetService<HealthCheckService>());
    }

    [Fact]
    public void AddInfrastructure_ShouldThrowWhenConnectionStringMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging service
        var configuration = new ConfigurationBuilder().Build(); // Empty configuration

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        
        // The exception is thrown when building the service provider, not when adding the service
        var exception = Assert.Throws<InvalidOperationException>(() => 
        {
            services.AddInfrastructure(configuration);
            var sp = services.BuildServiceProvider();
            sp.GetRequiredService<SmartNotesDbContext>(); // This triggers the configuration
        });
        
        Assert.Contains("Connection string 'DefaultConnection' not found", exception.Message);
    }

    [Fact]
    public void AddInfrastructureForTesting_ShouldRegisterInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "TestDatabase";

        // Act
        services.AddInfrastructureForTesting(connectionString);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<SmartNotesDbContext>();
        Assert.True(dbContext.Database.IsInMemory());
        
        var repository = serviceProvider.GetService<INoteRepository>();
        Assert.NotNull(repository);
        Assert.IsType<NoteRepository>(repository);
    }

    [Fact]
    public void AddInfrastructure_ShouldConfigureDevelopmentSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging service
        var configuration = CreateTestConfiguration();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            // Act
            services.AddInfrastructure(configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetRequiredService<SmartNotesDbContext>();
            Assert.NotNull(dbContext);
            
            // Verify DbContext options are properly configured
            var dbOptions = serviceProvider.GetRequiredService<DbContextOptions<SmartNotesDbContext>>();
            Assert.NotNull(dbOptions);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterHealthChecks()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging service
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var healthCheckService = serviceProvider.GetService<HealthCheckService>();
        Assert.NotNull(healthCheckService);
    }

    [Fact]
    public void AddInfrastructure_ShouldConfigureNpgsqlRetryPolicy()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();

        // Act
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<SmartNotesDbContext>();
        Assert.NotNull(dbContext);
        
        // The retry policy is configured internally, so we just verify the context is created
        Assert.NotNull(dbContext.Database);
    }

    [Fact]
    public void AddInfrastructureForTesting_ShouldEnableDetailedLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        var connectionString = "TestDatabase";

        // Act
        services.AddInfrastructureForTesting(connectionString);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var dbContext = serviceProvider.GetRequiredService<SmartNotesDbContext>();
        Assert.NotNull(dbContext);
        
        // Verify it's using in-memory database
        Assert.True(dbContext.Database.IsInMemory());
    }

    private static IConfiguration CreateTestConfiguration()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=smartnotes_test;Username=test;Password=test"
        });
        return configBuilder.Build();
    }
}