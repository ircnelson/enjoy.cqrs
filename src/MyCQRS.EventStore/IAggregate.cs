using System;
using System.Collections.Generic;
using MyCQRS.Events;

namespace MyCQRS.EventStore
{
    /// <summary>
    /// Abstraction of concept of the Aggregate with steroids (support events).
    /// </summary>
    public interface IAggregate
    {
        /// <summary>
        /// Unique identifier property.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The Aggregate version (used for Concurrency)
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Apply the event in Aggregate.
        /// The last event applied is the current state of the Aggregate.
        /// </summary>
        /// <param name="event"></param>
        void ApplyEvent(IDomainEvent @event);

        /// <summary>
        /// Load the events in the Aggregate.
        /// </summary>
        /// <param name="events"></param>
        void LoadFromHistory(IEnumerable<IDomainEvent> events);

        /// <summary>
        /// Collection of <see cref="IDomainEvent"/> that contains uncommited events.
        /// All events that not persisted yet should be here.
        /// </summary>
        IReadOnlyCollection<IDomainEvent> UncommitedEvents { get; }

        /// <summary>
        /// Clear the collection of events that uncommited.
        /// </summary>
        void ClearUncommitedEvents();
    }
}