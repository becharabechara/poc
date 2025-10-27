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
        Assert.Single(note.Tags);
        Assert.Equal("new", note.Tags.First().Value);
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
        var note = new Note("Title", null, null);
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
}