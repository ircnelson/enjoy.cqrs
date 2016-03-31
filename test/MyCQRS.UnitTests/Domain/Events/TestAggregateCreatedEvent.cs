using System;
using MyCQRS.Events;

namespace MyCQRS.UnitTests.Domain.Events
{
    public class TestAggregateCreatedEvent : DomainEvent
    {
        public TestAggregateCreatedEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}