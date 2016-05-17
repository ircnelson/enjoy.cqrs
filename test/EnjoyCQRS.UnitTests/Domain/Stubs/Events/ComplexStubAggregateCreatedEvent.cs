using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.AggregateWithEntities.Events
{
    public class ComplexStubAggregateCreatedEvent : DomainEvent
    {
        public string Name { get; }

        public ComplexStubAggregateCreatedEvent(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }
}