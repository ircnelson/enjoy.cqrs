using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Owin.IntegrationTests.Application.Domain.FooAggregate
{
    public class FooCreated : DomainEvent
    {
        public FooCreated(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}