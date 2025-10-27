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

    [Fact]
    public async Task AddAsync_ShouldThrowForNullNote()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowForNullNote()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null!));
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrueForExistingNote()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag>());
        await _repository.AddAsync(note);

        // Act
        var exists = await _repository.ExistsAsync(note.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalseForNonExistentNote()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var exists = await _repository.ExistsAsync(nonExistentId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldSupportCancellationToken()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag>());
        await _repository.AddAsync(note);
        using var cts = new CancellationTokenSource();

        // Act
        var exists = await _repository.ExistsAsync(note.Id, cts.Token);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var note1 = new Note("Note 1", "Content 1", new List<Tag>());
        var note2 = new Note("Note 2", "Content 2", new List<Tag>());
        var note3 = new Note("Note 3", "Content 3", new List<Tag>());

        await _repository.AddAsync(note1);
        await _repository.AddAsync(note2);
        await _repository.AddAsync(note3);

        // Act
        var count = await _repository.CountAsync();

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task CountAsync_ShouldSupportCancellationToken()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag>());
        await _repository.AddAsync(note);
        using var cts = new CancellationTokenSource();

        // Act
        var count = await _repository.CountAsync(cts.Token);

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetDistinctTagsAsync_ShouldReturnUniqueTagsSorted()
    {
        // Arrange
        var note1 = new Note("Note 1", "Content 1", new List<Tag> { new("c#"), new("programming") });
        var note2 = new Note("Note 2", "Content 2", new List<Tag> { new("javascript"), new("programming") });
        var note3 = new Note("Note 3", "Content 3", new List<Tag> { new("backend"), new("c#") });

        await _repository.AddAsync(note1);
        await _repository.AddAsync(note2);
        await _repository.AddAsync(note3);

        // Act
        var distinctTags = await _repository.GetDistinctTagsAsync();

        // Assert
        Assert.Equal(4, distinctTags.Count);
        Assert.Equal(new[] { "backend", "c#", "javascript", "programming" }, distinctTags.OrderBy(t => t));
    }

    [Fact]
    public async Task GetDistinctTagsAsync_ShouldReturnEmptyForNotesWithoutTags()
    {
        // Arrange
        var note = new Note("Note without tags", "Content", new List<Tag>());
        await _repository.AddAsync(note);

        // Act
        var distinctTags = await _repository.GetDistinctTagsAsync();

        // Assert
        Assert.Empty(distinctTags);
    }

    [Fact]
    public async Task GetDistinctTagsAsync_ShouldSupportCancellationToken()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag> { new("test") });
        await _repository.AddAsync(note);
        using var cts = new CancellationTokenSource();

        // Act
        var distinctTags = await _repository.GetDistinctTagsAsync(cts.Token);

        // Assert
        Assert.Single(distinctTags);
        Assert.Equal("test", distinctTags.First());
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            var note = new Note($"Note {i:D2}", $"Content {i}", new List<Tag>());
            await _repository.AddAsync(note);
            await Task.Delay(1); // Ensure different timestamps
        }

        // Act
        var firstPage = await _repository.GetAllAsync(1, 5);
        var secondPage = await _repository.GetAllAsync(2, 5);
        var thirdPage = await _repository.GetAllAsync(3, 5);

        // Assert
        Assert.Equal(5, firstPage.Count());
        Assert.Equal(5, secondPage.Count());
        Assert.Equal(5, thirdPage.Count());

        // Verify ordering (most recent first)
        var firstPageList = firstPage.ToList();
        Assert.True(firstPageList[0].UpdatedAt >= firstPageList[1].UpdatedAt);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldHandleInvalidParameters()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag>());
        await _repository.AddAsync(note);

        // Act
        var resultInvalidPage = await _repository.GetAllAsync(0, 5); // page < 1
        var resultInvalidPageSize = await _repository.GetAllAsync(1, 0); // pageSize < 1
        var resultLargePageSize = await _repository.GetAllAsync(1, 200); // pageSize > 100

        // Assert
        Assert.Single(resultInvalidPage); // Should default to page 1
        Assert.Single(resultInvalidPageSize); // Should default to pageSize 10
        Assert.Single(resultLargePageSize); // Should limit to pageSize 100
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldSupportCancellationToken()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag>());
        await _repository.AddAsync(note);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _repository.GetAllAsync(1, 10, cts.Token);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldHandleWhitespaceKeyword()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag>());
        await _repository.AddAsync(note);

        // Act
        var resultWithSpaces = await _repository.SearchAsync("   ", new List<string>());
        var resultWithTabs = await _repository.SearchAsync("\t\t", new List<string>());

        // Assert
        Assert.Single(resultWithSpaces); // Should return all notes when keyword is whitespace
        Assert.Single(resultWithTabs);
    }

    [Fact]
    public async Task SearchAsync_ShouldHandleEmptyTagsList()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag> { new("test") });
        await _repository.AddAsync(note);

        // Act
        var result = await _repository.SearchAsync("test", new List<string>());

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_ShouldHandleMultipleTags()
    {
        // Arrange
        var note1 = new Note("Note 1", "Content", new List<Tag> { new("tag1"), new("tag2"), new("tag3") });
        var note2 = new Note("Note 2", "Content", new List<Tag> { new("tag1"), new("tag2") });
        var note3 = new Note("Note 3", "Content", new List<Tag> { new("tag1") });

        await _repository.AddAsync(note1);
        await _repository.AddAsync(note2);
        await _repository.AddAsync(note3);

        // Act - Search for notes that have both tag1 AND tag2
        var result = await _repository.SearchAsync(null, new List<string> { "tag1", "tag2" });

        // Assert
        Assert.Equal(2, result.Count()); // Only note1 and note2 have both tags
        Assert.Contains(result, n => n.Title == "Note 1");
        Assert.Contains(result, n => n.Title == "Note 2");
    }

    [Fact]
    public async Task DeleteAsync_WithTrackedEntity_ShouldHandleEntityState()
    {
        // Arrange
        var note = new Note("Test Note", "Content", new List<Tag>());
        await _repository.AddAsync(note);

        // Use a separate context to avoid tracking conflicts
        using var newContext = CreateNewDbContext();
        var newRepository = new SmartNotes.Infrastructure.Repositories.NoteRepository(newContext);

        // Act
        await newRepository.DeleteAsync(note.Id);

        // Assert
        var deletedNote = await newRepository.GetByIdAsync(note.Id);
        Assert.Null(deletedNote);
    }

    [Fact]
    public void NoteRepository_Constructor_ShouldThrowForNullContext()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new SmartNotes.Infrastructure.Repositories.NoteRepository(null!));
    }
}