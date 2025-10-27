namespace SmartNotes.Application.DTOs;

public class SearchNotesRequest
{
    public string? Keyword { get; set; }
    public IEnumerable<string>? Tags { get; set; }
}