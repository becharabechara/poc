using SmartNotes.Infrastructure.Configuration;

namespace SmartNotes.Infrastructure.Tests.Configuration;

public class InfrastructureSettingsTests
{
    [Fact]
    public void InfrastructureSettings_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var settings = new InfrastructureSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.NotNull(settings.Database);
        Assert.IsType<DatabaseSettings>(settings.Database);
        Assert.False(settings.UseInMemoryDatabase);
        Assert.Equal("SmartNotesTestDb", settings.InMemoryDatabaseName);
    }

    [Fact]
    public void InfrastructureSettings_SectionName_ShouldBeCorrect()
    {
        // Act & Assert
        Assert.Equal("Infrastructure", InfrastructureSettings.SectionName);
    }

    [Fact]
    public void InfrastructureSettings_ShouldAllowSettingDatabaseSettings()
    {
        // Arrange
        var settings = new InfrastructureSettings();
        var databaseSettings = new DatabaseSettings
        {
            ConnectionString = "test-connection",
            CommandTimeout = 60,
            EnableSensitiveDataLogging = true
        };

        // Act
        settings.Database = databaseSettings;

        // Assert
        Assert.Equal(databaseSettings, settings.Database);
        Assert.Equal("test-connection", settings.Database.ConnectionString);
        Assert.Equal(60, settings.Database.CommandTimeout);
        Assert.True(settings.Database.EnableSensitiveDataLogging);
    }

    [Fact]
    public void InfrastructureSettings_Database_ShouldNotBeNullByDefault()
    {
        // Arrange & Act
        var settings = new InfrastructureSettings();

        // Assert
        Assert.NotNull(settings.Database);
    }

    [Fact]
    public void InfrastructureSettings_ShouldHandleNullDatabaseSettings()
    {
        // Arrange
        var settings = new InfrastructureSettings();

        // Act
        settings.Database = null!;

        // Assert
        Assert.Null(settings.Database);
    }

    [Fact]
    public void InfrastructureSettings_UseInMemoryDatabase_ShouldAllowToggling()
    {
        // Arrange
        var settings = new InfrastructureSettings();

        // Act & Assert
        Assert.False(settings.UseInMemoryDatabase); // Default

        settings.UseInMemoryDatabase = true;
        Assert.True(settings.UseInMemoryDatabase);

        settings.UseInMemoryDatabase = false;
        Assert.False(settings.UseInMemoryDatabase);
    }

    [Fact]
    public void InfrastructureSettings_InMemoryDatabaseName_ShouldAllowCustomNames()
    {
        // Arrange
        var settings = new InfrastructureSettings();

        // Act
        settings.InMemoryDatabaseName = "CustomTestDb";

        // Assert
        Assert.Equal("CustomTestDb", settings.InMemoryDatabaseName);
    }

    [Fact]
    public void InfrastructureSettings_InMemoryDatabaseName_ShouldHandleNullAndEmpty()
    {
        // Arrange
        var settings = new InfrastructureSettings();

        // Act & Assert
        settings.InMemoryDatabaseName = null!;
        Assert.Null(settings.InMemoryDatabaseName);

        settings.InMemoryDatabaseName = "";
        Assert.Equal("", settings.InMemoryDatabaseName);

        settings.InMemoryDatabaseName = "   ";
        Assert.Equal("   ", settings.InMemoryDatabaseName);
    }
}