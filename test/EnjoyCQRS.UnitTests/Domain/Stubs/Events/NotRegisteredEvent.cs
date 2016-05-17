using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.Events
{
    public class NotRegisteredEvent : DomainEvent
    {
        public NotRegisteredEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}