using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartNotes.Domain.Entities;

namespace SmartNotes.Application.Ports;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(Guid id);
    Task<IEnumerable<Note>> GetAllAsync();
    Task<IEnumerable<Note>> SearchAsync(string? keyword, IEnumerable<string>? tags);
    Task AddAsync(Note note);
    Task UpdateAsync(Note note);
    Task DeleteAsync(Guid id);
}