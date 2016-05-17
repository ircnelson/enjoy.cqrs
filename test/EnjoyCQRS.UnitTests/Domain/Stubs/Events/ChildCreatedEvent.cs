using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.AggregateWithEntities.Events
{
    public class ChildCreatedEvent : DomainEvent
    {
        public string Name { get; }

        public ChildCreatedEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }
    }
}