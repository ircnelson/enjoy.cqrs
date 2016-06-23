using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FooAggregate
{
    public class FooCreated : DomainEvent
    {
        public FooCreated(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}