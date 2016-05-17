using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs.Events
{
    public class NotRegisteredEvent : DomainEvent
    {
        public NotRegisteredEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}