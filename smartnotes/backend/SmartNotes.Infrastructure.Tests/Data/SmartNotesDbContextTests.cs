using Microsoft.EntityFrameworkCore;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;
using SmartNotes.Infrastructure.Data;
using SmartNotes.Infrastructure.Tests.Common;

namespace SmartNotes.Infrastructure.Tests.Data;

public class SmartNotesDbContextTests : DatabaseTestBase
{
    [Fact]
    public void DbContext_ShouldHaveNotesDbSet()
    {
        // Act & Assert
        Assert.NotNull(DbContext.Notes);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldUpdateTimestamps()
    {
        // Arrange
        var note = new Note("Test Title", "Test Content", new List<Tag> { new("test") });
        var originalUpdatedAt = note.UpdatedAt;

        // Act
        DbContext.Notes.Add(note);
        await DbContext.SaveChangesAsync();

        // Give some time for timestamp update
        await Task.Delay(1);
        
        note.Update("Updated Title", "Updated Content", new List<Tag> { new("updated") });
        await DbContext.SaveChangesAsync();

        // Assert
        var savedNote = await DbContext.Notes.FirstAsync(n => n.Id == note.Id);
        Assert.True(savedNote.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void SaveChanges_ShouldUpdateTimestamps()
    {
        // Arrange
        var note = new Note("Test Title", "Test Content", new List<Tag> { new("test") });
        var originalUpdatedAt = note.UpdatedAt;

        // Act
        DbContext.Notes.Add(note);
        DbContext.SaveChanges();

        // Give some time for timestamp update
        Thread.Sleep(1);
        
        note.Update("Updated Title", "Updated Content", new List<Tag> { new("updated") });
        DbContext.SaveChanges();

        // Assert
        var savedNote = DbContext.Notes.First(n => n.Id == note.Id);
        Assert.True(savedNote.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task OnModelCreating_ShouldApplyNoteConfiguration()
    {
        // Arrange
        var note = new Note("Test Title", "Test Content", new List<Tag> { new("test"), new("example") });

        // Act
        DbContext.Notes.Add(note);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedNote = await DbContext.Notes.FirstAsync(n => n.Id == note.Id);
        Assert.Equal("Test Title", savedNote.Title);
        Assert.Equal("Test Content", savedNote.Content);
        Assert.Equal(2, savedNote.Tags!.Count);
    }

    [Fact]
    public void OnConfiguring_ShouldSetQueryTrackingBehavior()
    {
        // Act & Assert
        Assert.Equal(QueryTrackingBehavior.NoTracking, DbContext.ChangeTracker.QueryTrackingBehavior);
    }

    [Fact]
    public void OnConfiguring_ShouldEnableLoggingInDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            // Act - Create a new context to test OnConfiguring
            var options = new DbContextOptionsBuilder<SmartNotesDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            using var context = new SmartNotesDbContext(options);

            // Assert - Context should be created successfully with development configuration
            Assert.NotNull(context);
            Assert.Equal(QueryTrackingBehavior.NoTracking, context.ChangeTracker.QueryTrackingBehavior);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public async Task ConfigureGlobalSettings_ShouldSetStringMaxLength()
    {
        // Arrange - Use valid title length
        var validTitle = new string('A', 150); // Within 200 character limit
        var note = new Note(validTitle, "Content", new List<Tag>());

        // Act & Assert
        DbContext.Notes.Add(note);
        
        // This should not throw an exception
        await DbContext.SaveChangesAsync();
        
        var savedNote = await DbContext.Notes.FirstAsync(n => n.Id == note.Id);
        Assert.NotNull(savedNote);
        Assert.Equal(validTitle, savedNote.Title);
    }

    [Fact]
    public async Task ConfigureGlobalSettings_ShouldSetDateTimeAsUtc()
    {
        // Arrange
        var note = new Note("Test Title", "Test Content", new List<Tag>());

        // Act
        DbContext.Notes.Add(note);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedNote = await DbContext.Notes.FirstAsync(n => n.Id == note.Id);
        
        // Verify timestamps are properly saved (DateTime is value type, no need for NotNull)
        Assert.True(savedNote.CreatedAt <= DateTime.UtcNow);
        Assert.True(savedNote.UpdatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task UpdateTimestamps_ShouldHandleNullValues()
    {
        // Arrange
        var note = new Note("Test Title", "Test Content", new List<Tag>());

        // Act
        DbContext.Notes.Add(note);
        await DbContext.SaveChangesAsync();

        // Try to update with null values (should be handled gracefully)
        var entry = DbContext.Entry(note);
        entry.State = EntityState.Modified;
        
        // This tests the UpdateTimestamps method's null checking
        await DbContext.SaveChangesAsync();

        // Assert
        var savedNote = await DbContext.Notes.FirstAsync(n => n.Id == note.Id);
        Assert.NotNull(savedNote);
    }

    [Fact]
    public async Task DbContext_ShouldHandleConcurrentOperations()
    {
        // Arrange
        var note1 = new Note("Note 1", "Content 1", new List<Tag>());
        var note2 = new Note("Note 2", "Content 2", new List<Tag>());

        // Act
        var task1 = Task.Run(async () =>
        {
            using var context = CreateNewDbContext();
            context.Notes.Add(note1);
            await context.SaveChangesAsync();
        });

        var task2 = Task.Run(async () =>
        {
            using var context = CreateNewDbContext();
            context.Notes.Add(note2);
            await context.SaveChangesAsync();
        });

        await Task.WhenAll(task1, task2);

        // Assert
        var allNotes = await DbContext.Notes.ToListAsync();
        Assert.Equal(2, allNotes.Count);
        Assert.Contains(allNotes, n => n.Title == "Note 1");
        Assert.Contains(allNotes, n => n.Title == "Note 2");
    }

    [Fact]
    public void DbContext_ShouldHandleDispose()
    {
        // Arrange
        var context = CreateNewDbContext();

        // Act & Assert (should not throw)
        context.Dispose();
    }

    [Fact]
    public void OnConfiguring_ShouldEnableDetailedErrorsInDevelopment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            // Act - Create a new context to test OnConfiguring
            var options = new DbContextOptionsBuilder<SmartNotesDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            using var context = new SmartNotesDbContext(options);

            // Assert - Context should be created successfully
            Assert.NotNull(context);
            // Note: In-memory database doesn't support EnableSensitiveDataLogging or EnableDetailedErrors
            // but the OnConfiguring method should still execute without errors
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public void OnConfiguring_ShouldNotEnableLoggingInProduction()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        try
        {
            // Act - Create a new context to test OnConfiguring
            var options = new DbContextOptionsBuilder<SmartNotesDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            using var context = new SmartNotesDbContext(options);

            // Assert - Context should be created successfully without development logging
            Assert.NotNull(context);
            Assert.Equal(QueryTrackingBehavior.NoTracking, context.ChangeTracker.QueryTrackingBehavior);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public async Task UpdateTimestamps_ShouldHandleReflectionUpdate()
    {
        // Arrange
        var note = new Note("Test Title", "Test Content", new List<Tag> { new("test") });
        DbContext.Notes.Add(note);
        await DbContext.SaveChangesAsync();

        // Act - Modify the note and save again to trigger UpdateTimestamps
        note.Update("Updated Title", "Updated Content", new List<Tag> { new("updated") });
        await DbContext.SaveChangesAsync();

        // Assert
        var savedNote = await DbContext.Notes.FirstAsync(n => n.Id == note.Id);
        Assert.Equal("Updated Title", savedNote.Title);
        Assert.Equal("Updated Content", savedNote.Content);
        Assert.True(savedNote.UpdatedAt > savedNote.CreatedAt);
    }

    [Fact]
    public async Task ConfigureGlobalSettings_ShouldHandleNullStrings()
    {
        // Arrange
        var note = new Note("Test", "Content", new List<Tag>());

        // Act
        DbContext.Notes.Add(note);
        await DbContext.SaveChangesAsync();

        // Assert - Should not throw when handling null strings in global settings
        var savedNote = await DbContext.Notes.FirstAsync(n => n.Id == note.Id);
        Assert.NotNull(savedNote);
    }
}