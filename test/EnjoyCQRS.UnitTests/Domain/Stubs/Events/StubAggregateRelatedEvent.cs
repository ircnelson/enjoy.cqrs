using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs.Events
{
    public class StubAggregateRelatedEvent : DomainEvent
    {
        public Guid StubAggregateId { get; }

        public StubAggregateRelatedEvent(Guid aggregateId, Guid stubAggregateId) : base(aggregateId)
        {
            StubAggregateId = stubAggregateId;
        }
    }
}