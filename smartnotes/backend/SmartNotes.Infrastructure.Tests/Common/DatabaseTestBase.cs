using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartNotes.Infrastructure.Data;
using SmartNotes.Infrastructure.Extensions;

namespace SmartNotes.Infrastructure.Tests.Common;

public abstract class DatabaseTestBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly SmartNotesDbContext DbContext;
    private readonly string _databaseName;

    protected DatabaseTestBase()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        var services = new ServiceCollection();
        
        // Use in-memory database for tests
        services.AddDbContext<SmartNotesDbContext>(options =>
        {
            options.UseInMemoryDatabase(_databaseName);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        // Add infrastructure services
        services.AddInfrastructureForTesting(_databaseName);

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<SmartNotesDbContext>();

        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }

    protected SmartNotesDbContext CreateNewDbContext()
    {
        var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SmartNotesDbContext>();
        return context;
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}