namespace SmartNotes.Infrastructure.Configuration;

public class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelay { get; set; } = 5;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
    public bool EnableQuerySplitting { get; set; } = true;
    public int MaxPoolSize { get; set; } = 100;
}

public class InfrastructureSettings
{
    public const string SectionName = "Infrastructure";

    public DatabaseSettings Database { get; set; } = new();
    public bool UseInMemoryDatabase { get; set; } = false;
    public string InMemoryDatabaseName { get; set; } = "SmartNotesTestDb";
}