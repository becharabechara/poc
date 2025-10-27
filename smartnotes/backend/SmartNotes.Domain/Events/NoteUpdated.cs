using System;

namespace SmartNotes.Domain.Events;

public class NoteUpdated : IDomainEvent
{
    public Guid NoteId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }

    public NoteUpdated(Guid noteId, string title)
    {
        NoteId = noteId;
        Title = title;
        OccurredOn = DateTime.UtcNow;
    }
}