using Microsoft.EntityFrameworkCore;
using SmartNotes.Application.Ports;
using SmartNotes.Domain.Entities;
using SmartNotes.Domain.ValueObjects;
using SmartNotes.Infrastructure.Data;

namespace SmartNotes.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly SmartNotesDbContext _context;

    public NoteRepository(SmartNotesDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        return await _context.Notes
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Note>> GetAllAsync()
    {
        return await _context.Notes
            .AsNoTracking()
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Note>> SearchAsync(string? keyword, IEnumerable<string>? tags)
    {
        var query = _context.Notes.AsNoTracking();

        // Apply text search if provided
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var trimmedKeyword = keyword.Trim();
            
            // Check if using PostgreSQL or In-Memory database
            if (_context.Database.IsInMemory())
            {
                // For In-Memory database, use standard string operations
                query = query.Where(n => n.Title!.ToLower().Contains(trimmedKeyword.ToLower()) ||
                                       n.Content!.ToLower().Contains(trimmedKeyword.ToLower()));
            }
            else
            {
                // For PostgreSQL, use ILike for case-insensitive search
                query = query.Where(n => EF.Functions.ILike(n.Title!, $"%{trimmedKeyword}%") ||
                                       EF.Functions.ILike(n.Content!, $"%{trimmedKeyword}%"));
            }
        }

        // Apply tag filtering if provided
        var tagList = tags?.ToList();
        if (tagList != null && tagList.Any())
        {
            if (_context.Database.IsInMemory())
            {
                // For In-Memory database, load data and filter in memory
                var allNotes = await query.ToListAsync();
                var filteredNotes = allNotes.Where(n => 
                    n.Tags != null && tagList.All(tag => 
                        n.Tags.Any(t => t.Value.Equals(tag, StringComparison.OrdinalIgnoreCase))))
                    .OrderByDescending(n => n.UpdatedAt);
                return filteredNotes;
            }
            else
            {
                // For PostgreSQL, use JSON operations
                query = query.Where(n => tagList.All(tag => 
                    EF.Functions.JsonContains(
                        EF.Property<string>(n, "tags"), 
                        $"\"{tag}\"")));
            }
        }

        return await query
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Note note)
    {
        if (note == null) throw new ArgumentNullException(nameof(note));

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Note note)
    {
        if (note == null) throw new ArgumentNullException(nameof(note));

        // Check if the note exists
        var existingNote = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == note.Id);

        if (existingNote == null)
        {
            throw new InvalidOperationException($"Note with ID {note.Id} not found.");
        }

        // Update the existing note properties
        _context.Entry(existingNote).CurrentValues.SetValues(note);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == id);

        if (note != null)
        {
            // Ensure the entity is not being tracked by other contexts
            var entry = _context.Entry(note);
            if (entry.State == EntityState.Detached)
            {
                _context.Notes.Attach(note);
            }
            
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
        }
    }

    // Additional helper methods for extended functionality
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .AsNoTracking()
            .AnyAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Notes.CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetDistinctTagsAsync(CancellationToken cancellationToken = default)
    {
        var allNotes = await _context.Notes
            .AsNoTracking()
            .Where(n => n.Tags != null && n.Tags.Any())
            .ToListAsync(cancellationToken);

        return allNotes
            .SelectMany(n => n.Tags!)
            .Select(t => t.Value)
            .Distinct()
            .OrderBy(tag => tag)
            .ToList()
            .AsReadOnly();
    }

    public async Task<IEnumerable<Note>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Max page size for performance

        return await _context.Notes
            .AsNoTracking()
            .OrderByDescending(n => n.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}