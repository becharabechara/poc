using System;

namespace SmartNotes.Application.DTOs;

public class UpdateNoteRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public IEnumerable<string> Tags { get; set; } = new List<string>();
}