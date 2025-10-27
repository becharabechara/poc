using System;

namespace SmartNotes.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}