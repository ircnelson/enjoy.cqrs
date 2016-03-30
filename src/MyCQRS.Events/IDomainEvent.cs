using System;

namespace MyCQRS.Events
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        Guid AggregateId { get; set; }
    }
}