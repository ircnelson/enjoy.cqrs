using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class NameChangedEvent : DomainEvent
    {
        public string Name { get; }

        public NameChangedEvent(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }
}