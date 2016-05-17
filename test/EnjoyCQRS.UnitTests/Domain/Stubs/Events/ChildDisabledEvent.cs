using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Domain.AggregateWithEntities.Events
{
    public class ChildDisabledEvent : DomainEvent
    {
        public ChildDisabledEvent(Guid entityId) : base(entityId)
        {
        }
    }
}