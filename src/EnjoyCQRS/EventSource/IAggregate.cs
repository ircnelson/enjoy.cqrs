using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventSource
{
    public interface IAggregate
    {
        Guid Id { get; }
        int EventVersion { get; }
        IReadOnlyCollection<IDomainEvent> UncommitedEvents { get; }
        int Version { get; }

        void ClearUncommitedEvents();
        void LoadFromHistory(IEnumerable<IDomainEvent> events);
    }
}