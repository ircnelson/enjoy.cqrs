using System;
using System.Collections.Generic;
using MyCQRS.Events;

namespace MyCQRS.EventStore
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get; }
        void ApplyEvent(IDomainEvent @event);
        void LoadFromHistory(IEnumerable<IDomainEvent> enumerable);
        void ClearUncommitedEvents();
    }
}