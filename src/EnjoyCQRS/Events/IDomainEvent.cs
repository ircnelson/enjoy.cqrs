using System;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Events
{
    /// <summary>
    /// Used to represent an Domain event.
    /// The domain event are things that have value for your domain.
    /// They are raised when occur changes on the aggregate root.
    /// </summary>
    public interface IDomainEvent : IMessage
    {
        /// <summary>
        /// Domain Event Unique identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Aggregate Unique identifier.
        /// </summary>
        Guid AggregateId { get; }

        /// <summary>
        /// Event version.
        /// </summary>
        int Version { get; }
    }
}