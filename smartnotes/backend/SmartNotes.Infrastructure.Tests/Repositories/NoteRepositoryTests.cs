using Microsoft.Extensions.DependencyInjection;
using SmartNotes.Application.Ports;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;
using SmartNotes.Infrastructure.Tests.Common;

namespace SmartNotes.Infrastructure.Tests.Repositories;

public class NoteRepositoryTests : DatabaseTestBase
{
    private readonly INoteRepository _repository;

    public NoteRepositoryTests()
    {
        _repository = ServiceProvider.GetRequiredService<INoteRepository>();
    }

    [Fact]
    public async Task AddAsync_ShouldAddNoteToDatabase()
    {
        // Arrange
        var tags = new List<Tag> { new("test"), new("integration") };
        var note = new Note("Test Title", "Test Content", tags);

        // Act
        await _repository.AddAsync(note);

        // Assert
        var retrievedNote = await _repository.GetByIdAsync(note.Id);
        Assert.NotNull(retrievedNote);
        Assert.Equal("Test Title", retrievedNote.Title);
        Assert.Equal("Test Content", retrievedNote.Content);
        Assert.Equal(2, retrievedNote.Tags!.Count);
        Assert.Contains(retrievedNote.Tags, t => t.Value == "test");
        Assert.Contains(retrievedNote.Tags, t => t.Value == "integration");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForNonExistentNote()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNotesOrderedByUpdatedAtDescending()
    {
        // Arrange
        var note1 = new Note("First Note", "Content 1", new List<Tag> { new("tag1") });
        var note2 = new Note("Second Note", "Content 2", new List<Tag> { new("tag2") });
        var note3 = new Note("Third Note", "Content 3", new List<Tag> { new("tag3") });

        await _repository.AddAsync(note1);
        await Task.Delay(10); // Ensure different timestamps
        await _repository.AddAsync(note2);
        await Task.Delay(10);
        await _repository.AddAsync(note3);

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, results.Count());
        var resultsList = results.ToList();
        Assert.Equal("Third Note", resultsList[0].Title); // Most recent first
        Assert.Equal("Second Note", resultsList[1].Title);
        Assert.Equal("First Note", resultsList[2].Title);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingNote()
    {
        // Arrange
        var originalTags = new List<Tag> { new("original") };
        var note = new Note("Original Title", "Original Content", originalTags);
        await _repository.AddAsync(note);

        // Act
        var updatedTags = new List<Tag> { new("updated"), new("modified") };
        note.Update("Updated Title", "Updated Content", updatedTags);
        await _repository.UpdateAsync(note);

        // Assert
        var retrievedNote = await _repository.GetByIdAsync(note.Id);
        Assert.NotNull(retrievedNote);
        Assert.Equal("Updated Title", retrievedNote.Title);
        Assert.Equal("Updated Content", retrievedNote.Content);
        Assert.Equal(2, retrievedNote.Tags!.Count);
        Assert.Contains(retrievedNote.Tags, t => t.Value == "updated");
        Assert.Contains(retrievedNote.Tags, t => t.Value == "modified");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForNonExistentNote()
    {
        // Arrange
        var note = new Note("Test Title", "Test Content", new List<Tag>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateAsync(note));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveNoteFromDatabase()
    {
        // Arrange
        var note = new Note("To Delete", "Content", new List<Tag>());
        await _repository.AddAsync(note);

        // Use a fresh context to avoid tracking issues
        using var newContext = CreateNewDbContext();
        var newRepository = new SmartNotes.Infrastructure.Repositories.NoteRepository(newContext);

        // Act
        await newRepository.DeleteAsync(note.Id);

        // Assert
        var retrievedNote = await newRepository.GetByIdAsync(note.Id);
        Assert.Null(retrievedNote);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrowForNonExistentNote()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert (should not throw)
        await _repository.DeleteAsync(nonExistentId);
    }

    [Fact]
    public async Task SearchAsync_ShouldFindNotesByKeyword()
    {
        // Arrange
        var note1 = new Note("JavaScript Basics", "Learn programming fundamentals", new List<Tag> { new("frontend") });
        var note2 = new Note("Python Guide", "Python programming tutorial", new List<Tag> { new("backend") });
        var note3 = new Note("Cooking Recipe", "How to cook pasta", new List<Tag> { new("cooking") });

        await _repository.AddAsync(note1);
        await _repository.AddAsync(note2);
        await _repository.AddAsync(note3);

        // Act
        var results = await _repository.SearchAsync("programming", new List<string>());

        // Assert
        Assert.Equal(2, results.Count());
        Assert.Contains(results, n => n.Title == "JavaScript Basics");
        Assert.Contains(results, n => n.Title == "Python Guide");
    }

    [Fact]
    public async Task SearchAsync_ShouldFindNotesByTags()
    {
        // Arrange
        var note1 = new Note("JavaScript Note", "Content 1", new List<Tag> { new("javascript"), new("frontend") });
        var note2 = new Note("Python Note", "Content 2", new List<Tag> { new("python"), new("backend") });
        var note3 = new Note("React Note", "Content 3", new List<Tag> { new("react"), new("frontend") });

        await _repository.AddAsync(note1);
        await _repository.AddAsync(note2);
        await _repository.AddAsync(note3);

        // Act
        var results = await _repository.SearchAsync(null, new List<string> { "frontend" });

        // Assert
        Assert.Equal(2, results.Count());
        Assert.Contains(results, n => n.Title == "JavaScript Note");
        Assert.Contains(results, n => n.Title == "React Note");
    }

    [Fact]
    public async Task SearchAsync_ShouldFindNotesByKeywordAndTags()
    {
        // Arrange
        var note1 = new Note("JavaScript Frontend", "Frontend development", new List<Tag> { new("javascript"), new("frontend") });
        var note2 = new Note("JavaScript Backend", "Backend development", new List<Tag> { new("javascript"), new("backend") });
        var note3 = new Note("Python Frontend", "Python frontend", new List<Tag> { new("python"), new("frontend") });

        await _repository.AddAsync(note1);
        await _repository.AddAsync(note2);
        await _repository.AddAsync(note3);

        // Act
        var results = await _repository.SearchAsync("frontend", new List<string> { "javascript" });

        // Assert
        Assert.Single(results);
        Assert.Equal("JavaScript Frontend", results.First().Title);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyKeywordAndTags_ShouldReturnAllNotes()
    {
        // Arrange
        var note1 = new Note("Note 1", "Content 1", new List<Tag>());
        var note2 = new Note("Note 2", "Content 2", new List<Tag>());

        await _repository.AddAsync(note1);
        await _repository.AddAsync(note2);

        // Act
        var results = await _repository.SearchAsync("", new List<string>());

        // Assert
        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task SearchAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var note = new Note("JavaScript Basics", "Learn JAVASCRIPT fundamentals", new List<Tag> { new("Programming") });
        await _repository.AddAsync(note);

        // Act
        var results1 = await _repository.SearchAsync("javascript", new List<string>());
        var results2 = await _repository.SearchAsync("JAVASCRIPT", new List<string>());
        var results3 = await _repository.SearchAsync("", new List<string> { "programming" });

        // Assert
        Assert.Single(results1);
        Assert.Single(results2);
        Assert.Single(results3);
    }
}
}