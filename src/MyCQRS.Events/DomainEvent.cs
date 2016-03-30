using System;

namespace MyCQRS.Events
{
    public class DomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public Guid AggregateId { get; set; }

        public DomainEvent()
        {
            Id = Guid.NewGuid();
        }
    }
}