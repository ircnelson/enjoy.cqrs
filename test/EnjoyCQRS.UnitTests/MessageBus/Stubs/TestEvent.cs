using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.MessageBus.Stubs
{
    public class TestEvent : DomainEvent
    {
        public TestEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}