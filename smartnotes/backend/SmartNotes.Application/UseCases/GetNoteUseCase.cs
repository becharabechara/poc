using System;
using System.Linq;
using System.Threading.Tasks;
using SmartNotes.Application.DTOs;
using SmartNotes.Application.Ports;

namespace SmartNotes.Application.UseCases;

public class GetNoteUseCase
{
    private readonly INoteRepository _noteRepository;

    public GetNoteUseCase(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<NoteResponse?> ExecuteAsync(Guid id)
    {
        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null) return null;

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