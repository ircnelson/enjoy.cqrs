using System;
using EnjoyCQRS.Attributes;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs.Events
{
    [EventName("StubCreated")]
    public class StubAggregateCreatedEvent : DomainEvent
    {
        public string Name { get; }

        public StubAggregateCreatedEvent(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }
}