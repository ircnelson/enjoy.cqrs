using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Stubs.DomainLayer
{
    public class FakePersonCreated : DomainEvent
    {
        public string Name { get; }

        public FakePersonCreated(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }
}