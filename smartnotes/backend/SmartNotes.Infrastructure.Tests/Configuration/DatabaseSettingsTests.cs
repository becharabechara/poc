using SmartNotes.Infrastructure.Configuration;

namespace SmartNotes.Infrastructure.Tests.Configuration;

public class DatabaseSettingsTests
{
    [Fact]
    public void DatabaseSettings_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var settings = new DatabaseSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(string.Empty, settings.ConnectionString);
        Assert.Equal(30, settings.CommandTimeout);
        Assert.Equal(3, settings.MaxRetryCount);
        Assert.Equal(5, settings.MaxRetryDelay);
        Assert.False(settings.EnableSensitiveDataLogging);
        Assert.False(settings.EnableDetailedErrors);
        Assert.True(settings.EnableQuerySplitting);
        Assert.Equal(100, settings.MaxPoolSize);
    }

    [Fact]
    public void DatabaseSettings_ShouldAllowSettingProperties()
    {
        // Arrange
        var settings = new DatabaseSettings();
        var connectionString = "Host=localhost;Database=test;Username=user;Password=pass";

        // Act
        settings.ConnectionString = connectionString;
        settings.CommandTimeout = 60;
        settings.MaxRetryCount = 5;
        settings.MaxRetryDelay = 10;
        settings.EnableSensitiveDataLogging = true;
        settings.EnableDetailedErrors = true;
        settings.EnableQuerySplitting = false;
        settings.MaxPoolSize = 200;

        // Assert
        Assert.Equal(connectionString, settings.ConnectionString);
        Assert.Equal(60, settings.CommandTimeout);
        Assert.Equal(5, settings.MaxRetryCount);
        Assert.Equal(10, settings.MaxRetryDelay);
        Assert.True(settings.EnableSensitiveDataLogging);
        Assert.True(settings.EnableDetailedErrors);
        Assert.False(settings.EnableQuerySplitting);
        Assert.Equal(200, settings.MaxPoolSize);
    }

    [Fact]
    public void DatabaseSettings_SectionName_ShouldBeCorrect()
    {
        // Act & Assert
        Assert.Equal("Database", DatabaseSettings.SectionName);
    }

    [Fact]
    public void DatabaseSettings_ConnectionString_ShouldHandleNullAndEmpty()
    {
        // Arrange
        var settings = new DatabaseSettings();

        // Act & Assert
        settings.ConnectionString = null!;
        Assert.Null(settings.ConnectionString);

        settings.ConnectionString = "";
        Assert.Equal("", settings.ConnectionString);

        settings.ConnectionString = "   ";
        Assert.Equal("   ", settings.ConnectionString);
    }

    [Fact]
    public void DatabaseSettings_CommandTimeout_ShouldHandleVariousValues()
    {
        // Arrange
        var settings = new DatabaseSettings();

        // Act & Assert
        settings.CommandTimeout = 0;
        Assert.Equal(0, settings.CommandTimeout);

        settings.CommandTimeout = 30;
        Assert.Equal(30, settings.CommandTimeout);

        settings.CommandTimeout = int.MaxValue;
        Assert.Equal(int.MaxValue, settings.CommandTimeout);
    }

    [Fact]
    public void DatabaseSettings_MaxRetryCount_ShouldHandleVariousValues()
    {
        // Arrange
        var settings = new DatabaseSettings();

        // Act & Assert
        settings.MaxRetryCount = 0;
        Assert.Equal(0, settings.MaxRetryCount);

        settings.MaxRetryCount = 10;
        Assert.Equal(10, settings.MaxRetryCount);
    }

    [Fact]
    public void DatabaseSettings_MaxRetryDelay_ShouldHandleVariousValues()
    {
        // Arrange
        var settings = new DatabaseSettings();

        // Act & Assert
        settings.MaxRetryDelay = 1;
        Assert.Equal(1, settings.MaxRetryDelay);

        settings.MaxRetryDelay = 30;
        Assert.Equal(30, settings.MaxRetryDelay);
    }

    [Fact]
    public void DatabaseSettings_MaxPoolSize_ShouldHandleVariousValues()
    {
        // Arrange
        var settings = new DatabaseSettings();

        // Act & Assert
        settings.MaxPoolSize = 1;
        Assert.Equal(1, settings.MaxPoolSize);

        settings.MaxPoolSize = 500;
        Assert.Equal(500, settings.MaxPoolSize);
    }
}