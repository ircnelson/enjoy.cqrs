using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Stubs.DomainLayer
{
    public class NameChanged : DomainEvent
    {
        public string Name { get; }

        public NameChanged(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }
}