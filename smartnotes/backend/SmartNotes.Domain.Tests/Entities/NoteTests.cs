using System;
using System.Linq;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;
using Xunit;

namespace SmartNotes.Domain.Tests.Entities;

public class NoteTests
{
    [Fact]
    public void CreateNote_WithValidData_ShouldSucceed()
    {
        var title = "Test Note";
        var content = "Test content";
        var tags = new[] { new Tag("test") };

        var before = DateTime.UtcNow;
        var note = new Note(title, content, tags);
        var after = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, note.Id);
        Assert.Equal(title, note.Title);
        Assert.Equal(content, note.Content);
        Assert.NotNull(note.Tags);
        Assert.Single(note.Tags!);
        Assert.True(note.CreatedAt >= before && note.CreatedAt <= after);
        Assert.Equal(note.CreatedAt, note.UpdatedAt);
    }

    [Fact]
    public void CreateNote_WithEmptyTitle_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => new Note("", "content", null));
    }

    [Fact]
    public void CreateNote_WithTitleTooLong_ShouldThrowException()
    {
        var longTitle = new string('a', 201);
        Assert.Throws<ArgumentException>(() => new Note(longTitle, "content", null));
    }

    [Fact]
    public void CreateNote_WithDuplicateTags_ShouldThrowException()
    {
        var tags = new[] { new Tag("test"), new Tag("test") };
        Assert.Throws<ArgumentException>(() => new Note("Title", "content", tags));
    }

    [Fact]
    public void UpdateNote_WithValidData_ShouldUpdate()
    {
        var note = new Note("Old Title", "Old content", new[] { new Tag("old") });
        var newTitle = "New Title";
        var newContent = "New content";
        var newTags = new[] { new Tag("new") };

        note.Update(newTitle, newContent, newTags);

        Assert.Equal(newTitle, note.Title);
        Assert.Equal(newContent, note.Content);
        Assert.Single(note.Tags!);
        Assert.Equal("new", note.Tags!.First().Value);
        Assert.True(note.UpdatedAt > note.CreatedAt);
    }

    [Fact]
    public void CreateNote_WithTitleOnlyWhitespace_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => new Note("   ", "content", null));
    }

    [Fact]
    public void CreateNote_WithNullContent_ShouldSetEmptyContent()
    {
        var note = new Note("Title", null!, null);
        Assert.Equal(string.Empty, note.Content);
    }

    [Fact]
    public void CreateNote_WithNoTags_ShouldHaveEmptyTags()
    {
        var note = new Note("Title", "content", null);
        Assert.Empty(note.Tags!);
    }

    [Fact]
    public void UpdateNote_WithNullTags_ShouldClearTags()
    {
        var note = new Note("Title", "content", new[] { new Tag("old") });
        note.Update("New Title", "new content", null);
        Assert.Empty(note.Tags!);
    }

    [Fact]
    public void UpdateNote_WithDuplicateTags_ShouldThrowException()
    {
        var note = new Note("Title", "content", null);
        var tags = new[] { new Tag("test"), new Tag("test") };
        Assert.Throws<ArgumentException>(() => note.Update("New Title", "content", tags));
    }

    [Fact]
    public void CreateNote_WithNullTitle_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => new Note(null!, "content", null));
    }

    [Fact]
    public void CreateNote_WithEmptyContent_ShouldSucceed()
    {
        var note = new Note("Title", "", null);
        Assert.Equal("", note.Content);
    }

    [Fact]
    public void UpdateNote_WithNullTitle_ShouldThrowException()
    {
        var note = new Note("Title", "content", null);
        Assert.Throws<ArgumentException>(() => note.Update(null!, "content", null));
    }

    [Fact]
    public void UpdateNote_WithEmptyTitle_ShouldThrowException()
    {
        var note = new Note("Title", "content", null);
        Assert.Throws<ArgumentException>(() => note.Update("", "content", null));
    }

    [Fact]
    public void UpdateNote_WithTitleTooLong_ShouldThrowException()
    {
        var note = new Note("Title", "content", null);
        var longTitle = new string('a', 201);
        Assert.Throws<ArgumentException>(() => note.Update(longTitle, "content", null));
    }

    [Fact]
    public void UpdateNote_WithNullContent_ShouldSetEmptyContent()
    {
        var note = new Note("Title", "old content", null);
        note.Update("New Title", null!, null);
        Assert.Equal("", note.Content);
    }

    [Fact]
    public void CreateNote_WithMaxLengthTitle_ShouldSucceed()
    {
        var maxTitle = new string('a', 200);
        var note = new Note(maxTitle, "content", null);
        Assert.Equal(maxTitle, note.Title);
    }

    [Fact]
    public void CreateNote_WithMultipleTags_ShouldSucceed()
    {
        var tags = new[] { new Tag("tag1"), new Tag("tag2"), new Tag("tag3") };
        var note = new Note("Title", "content", tags);
        Assert.Equal(3, note.Tags!.Count);
    }

    [Fact]
    public void UpdateNote_ShouldPreserveId()
    {
        var note = new Note("Title", "content", null);
        var originalId = note.Id;
        
        note.Update("New Title", "new content", null);
        
        Assert.Equal(originalId, note.Id);
    }

    [Fact]
    public void UpdateNote_ShouldPreserveCreatedAt()
    {
        var note = new Note("Title", "content", null);
        var originalCreatedAt = note.CreatedAt;
        
        note.Update("New Title", "new content", null);
        
        Assert.Equal(originalCreatedAt, note.CreatedAt);
    }

    [Fact]
    public void CreateNote_ShouldGenerateUniqueIds()
    {
        var note1 = new Note("Title 1", "content", null);
        var note2 = new Note("Title 2", "content", null);
        
        Assert.NotEqual(note1.Id, note2.Id);
    }

    [Fact]
    public void CreateNote_WithTitleExactly200Characters_ShouldSucceed()
    {
        var exactTitle = new string('a', 200);
        var note = new Note(exactTitle, "content", null);
        Assert.Equal(exactTitle, note.Title);
    }

    [Fact]
    public void UpdateNote_WithTitleExactly200Characters_ShouldSucceed()
    {
        var note = new Note("Title", "content", null);
        var exactTitle = new string('a', 200);
        note.Update(exactTitle, "new content", null);
        Assert.Equal(exactTitle, note.Title);
    }

    [Fact]
    public void CreateNote_WithTagsContainingDuplicates_ShouldThrowException()
    {
        var tags = new[] { new Tag("test"), new Tag("Test"), new Tag("TEST") }; // Different casing but same normalized value
        Assert.Throws<ArgumentException>(() => new Note("Title", "content", tags));
    }

    [Fact]
    public void CreateNote_WithEmptyTagsList_ShouldSucceed()
    {
        var tags = new Tag[0];
        var note = new Note("Title", "content", tags);
        Assert.Empty(note.Tags!);
    }

    [Fact]
    public void UpdateNote_WithEmptyTagsList_ShouldClearTags()
    {
        var note = new Note("Title", "content", new[] { new Tag("old") });
        var emptyTags = new Tag[0];
        note.Update("New Title", "content", emptyTags);
        Assert.Empty(note.Tags!);
    }

    [Fact]
    public void CreateNote_WithTagsHavingDifferentCases_ShouldThrowExceptionForDuplicates()
    {
        var tags = new[] { new Tag("Test"), new Tag("test") };
        Assert.Throws<ArgumentException>(() => new Note("Title", "content", tags));
    }

    [Fact]
    public void Note_ShouldBeImmutableExceptThroughUpdate()
    {
        var note = new Note("Title", "content", new[] { new Tag("test") });
        
        // Properties should be read-only
        Assert.Equal("Title", note.Title);
        Assert.Equal("content", note.Content);
        Assert.Single(note.Tags!);
        
        // Only Update method should change the note
        note.Update("New Title", "new content", new[] { new Tag("new") });
        
        Assert.Equal("New Title", note.Title);
        Assert.Equal("new content", note.Content);
        Assert.Single(note.Tags!);
        Assert.Equal("new", note.Tags!.First().Value);
    }

    [Fact]
    public void CreateNote_WithVeryLongContent_ShouldSucceed()
    {
        var longContent = new string('a', 10000);
        var note = new Note("Title", longContent, null);
        Assert.Equal(longContent, note.Content);
    }

    [Fact]
    public void UpdateNote_WithVeryLongContent_ShouldSucceed()
    {
        var note = new Note("Title", "short content", null);
        var longContent = new string('a', 10000);
        note.Update("Title", longContent, null);
        Assert.Equal(longContent, note.Content);
    }
}