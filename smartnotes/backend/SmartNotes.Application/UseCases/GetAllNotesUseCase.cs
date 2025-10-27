using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartNotes.Application.DTOs;
using SmartNotes.Application.Ports;

namespace SmartNotes.Application.UseCases;

public class GetAllNotesUseCase
{
    private readonly INoteRepository _noteRepository;

    public GetAllNotesUseCase(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<IEnumerable<NoteResponse>> ExecuteAsync()
    {
        var notes = await _noteRepository.GetAllAsync();

        return notes.Select(note => new NoteResponse
        {
            Id = note.Id,
            Title = note.Title!,
            Content = note.Content!,
            Tags = note.Tags!.Select(t => t.Value),
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        });
    }
}