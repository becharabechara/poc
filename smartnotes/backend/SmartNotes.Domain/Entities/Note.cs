using System;
using System.Collections.Generic;
using System.Linq;
using SmartNotes.Domain.ValueObjects;

namespace SmartNotes.Domain.Entities;

public class Note
{
    public Guid Id { get; private set; }
    public string? Title { get; private set; }
    public string? Content { get; private set; }
    public IReadOnlyCollection<Tag>? Tags { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Note() 
    {
        // EF Core requires a parameterless constructor
    }

    public Note(string title, string content, IEnumerable<Tag>? tags)
    {
        Id = Guid.NewGuid();
        SetTitle(title);
        SetContent(content);
        SetTags(tags);
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void Update(string title, string content, IEnumerable<Tag>? tags)
    {
        SetTitle(title);
        SetContent(content);
        SetTags(tags);
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (title.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));
        Title = title;
    }

    private void SetContent(string content)
    {
        Content = content ?? string.Empty;
    }

    private void SetTags(IEnumerable<Tag>? tags)
    {
        var tagList = tags?.ToList() ?? new List<Tag>();
        if (tagList.Count != tagList.Distinct().Count())
            throw new ArgumentException("Tags must be unique.", nameof(tags));
        Tags = tagList.AsReadOnly();
    }
}