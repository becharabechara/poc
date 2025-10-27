using System;
using System.Threading.Tasks;
using SmartNotes.Application.Ports;

namespace SmartNotes.Application.UseCases;

public class DeleteNoteUseCase
{
    private readonly INoteRepository _noteRepository;

    public DeleteNoteUseCase(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<bool> ExecuteAsync(Guid id)
    {
        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null) return false;

        await _noteRepository.DeleteAsync(id);
        return true;
    }
}