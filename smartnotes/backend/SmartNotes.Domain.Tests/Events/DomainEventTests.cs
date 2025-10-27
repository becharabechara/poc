using System;
using SmartNotes.Domain.Events;
using Xunit;

namespace SmartNotes.Domain.Tests.Events;

public class DomainEventTests
{
    [Fact]
    public void NoteCreated_ShouldSetPropertiesCorrectly()
    {
        var noteId = Guid.NewGuid();
        var title = "Test Note";

        var @event = new NoteCreated(noteId, title);

        Assert.Equal(noteId, @event.NoteId);
        Assert.Equal(title, @event.Title);
        Assert.True(@event.OccurredOn <= DateTime.UtcNow);
    }

    [Fact]
    public void NoteUpdated_ShouldSetPropertiesCorrectly()
    {
        var noteId = Guid.NewGuid();
        var title = "Updated Note";

        var @event = new NoteUpdated(noteId, title);

        Assert.Equal(noteId, @event.NoteId);
        Assert.Equal(title, @event.Title);
        Assert.True(@event.OccurredOn <= DateTime.UtcNow);
    }
}