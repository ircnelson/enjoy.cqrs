using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Owin.IntegrationTests.Application.Domain.FooAggregate
{
    public class DidSomething : DomainEvent
    {
        public DidSomething(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}