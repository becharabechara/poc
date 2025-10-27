using System;
using System.Collections.Generic;
using System.Threading;
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
    
    // Extended functionality for improved coverage
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetDistinctTagsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Note>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}