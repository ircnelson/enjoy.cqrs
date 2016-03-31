using System;
using MyCQRS.Events;

namespace MyCQRS.UnitTests.Domain.Events
{
    public class NotRegisteredEvent : DomainEvent
    {
        public NotRegisteredEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}