using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.Events
{
    public class TestAggregateCreatedEvent : DomainEvent
    {
        public TestAggregateCreatedEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}