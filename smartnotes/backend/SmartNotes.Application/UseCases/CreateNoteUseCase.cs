using System.Linq;
using System.Threading.Tasks;
using SmartNotes.Application.DTOs;
using SmartNotes.Application.Ports;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;

namespace SmartNotes.Application.UseCases;

public class CreateNoteUseCase
{
    private readonly INoteRepository _noteRepository;

    public CreateNoteUseCase(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<NoteResponse> ExecuteAsync(CreateNoteRequest request)
    {
        var tags = (request.Tags ?? Enumerable.Empty<string>()).Select(t => new Tag(t)).ToList();
        var note = new Note(request.Title, request.Content, tags);

        await _noteRepository.AddAsync(note);

        return new NoteResponse
        {
            Id = note.Id,
            Title = note.Title!,
            Content = note.Content!,
            Tags = note.Tags!.Select(t => t.Value),
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}