using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class FakePersonCreatedEvent : DomainEvent
    {
        public string Name { get; }

        public FakePersonCreatedEvent(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }
}