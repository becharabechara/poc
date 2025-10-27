using System.Collections.Generic;

namespace SmartNotes.Application.DTOs;

public class CreateNoteRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public IEnumerable<string> Tags { get; set; } = new List<string>();
}