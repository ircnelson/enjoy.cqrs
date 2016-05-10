using System;

namespace EnjoyCQRS.Events
{
    /// <summary>
    /// Used to represent an Domain event.
    /// The domain event are things that have value for your domain.
    /// They are raised when occur changes on the aggregate root.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Domain Event Unique identifier.
        /// </summary>
        public Guid Id { get; internal set; }

        /// <summary>
        /// Aggregate Unique identifier.
        /// </summary>
        public Guid AggregateId { get; }

        /// <summary>
        /// Event version.
        /// </summary>
        public int Version { get; internal set; }

        /// <summary>
        /// Construct the domain event.
        /// </summary>
        /// <param name="aggregateId"></param>
        protected DomainEvent(Guid aggregateId)
        {
            Id = Guid.NewGuid();

            AggregateId = aggregateId;
        }
    }
}