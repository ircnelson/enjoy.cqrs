using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs.Events
{
    public class ChildCreatedEvent : DomainEvent
    {
        public Guid EntityId { get; }
        public string Name { get; }

        public ChildCreatedEvent(Guid aggregateId, Guid entityId, string name) : base(aggregateId)
        {
            EntityId = entityId;
            Name = name;
        }
    }
}