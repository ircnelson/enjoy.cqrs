using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs.Events
{
    public class TestAggregateCreatedEvent : DomainEvent
    {
        public TestAggregateCreatedEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}