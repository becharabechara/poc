using System;

namespace SmartNotes.Domain.ValueObjects;

public record Tag
{
    public string Value { get; }

    public Tag(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tag cannot be empty.", nameof(value));
        if (value.Length > 50)
            throw new ArgumentException("Tag cannot exceed 50 characters.", nameof(value));
        Value = value.ToLowerInvariant(); // Normalize to lowercase for consistency
    }

    public static implicit operator string(Tag tag) => tag.Value;
    public static explicit operator Tag(string value) => new Tag(value);
}