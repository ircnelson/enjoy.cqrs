using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs.Events
{
    public class ChildDisabledEvent : DomainEvent
    {
        public Guid EntityId { get; }

        public ChildDisabledEvent(Guid aggregateId, Guid entityId) : base(aggregateId)
        {
            EntityId = entityId;
        }
    }
}