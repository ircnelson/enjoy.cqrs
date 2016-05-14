using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.MessageBus.Stubs
{
    public class OrderedTestEvent : DomainEvent
    {
        public int Order { get; }

        public OrderedTestEvent(Guid aggregateId, int order) : base(aggregateId)
        {
            Order = order;
        }
    }
}