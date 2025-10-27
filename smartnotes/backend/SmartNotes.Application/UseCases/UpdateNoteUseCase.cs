using System.Linq;
using System.Threading.Tasks;
using SmartNotes.Application.DTOs;
using SmartNotes.Application.Ports;
using SmartNotes.Domain.ValueObjects;

namespace SmartNotes.Application.UseCases;

public class UpdateNoteUseCase
{
    private readonly INoteRepository _noteRepository;

    public UpdateNoteUseCase(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<NoteResponse?> ExecuteAsync(UpdateNoteRequest request)
    {
        var note = await _noteRepository.GetByIdAsync(request.Id);
        if (note == null) return null;

        var tags = (request.Tags ?? Enumerable.Empty<string>()).Select(t => new Tag(t)).ToList();
        note.Update(request.Title, request.Content, tags);

        await _noteRepository.UpdateAsync(note);

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