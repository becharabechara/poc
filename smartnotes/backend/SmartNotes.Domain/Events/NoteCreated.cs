using System;

namespace SmartNotes.Domain.Events;

public class NoteCreated : IDomainEvent
{
    public Guid NoteId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }

    public NoteCreated(Guid noteId, string title)
    {
        NoteId = noteId;
        Title = title;
        OccurredOn = DateTime.UtcNow;
    }
}