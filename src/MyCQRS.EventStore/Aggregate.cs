using System;
using System.Collections.Generic;
using System.Linq;
using MyCQRS.Events;

namespace MyCQRS.EventStore
{
    public abstract class Aggregate : IAggregate
    {
        private readonly List<IDomainEvent> _uncommitedEvents = new List<IDomainEvent>();
        private readonly Route<IDomainEvent> _routeEvents = new Route<IDomainEvent>();

        public IReadOnlyCollection<IDomainEvent> UncommitedEvents => _uncommitedEvents.AsReadOnly();
        
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Current version of the Aggregate.
        /// </summary>
        public int Version { get; protected set; }

        /// <summary>
        /// This version is calculated based on Uncommited events.
        /// </summary>
        public int EventVersion { get; protected set; }

        protected void On<T>(Action<T> action)
            where T : class 
        {
            _routeEvents.Add(typeof(T), o => action(o as T));
        }

        protected void Raise(IDomainEvent @event)
        {
            ApplyEvent(@event);
            _uncommitedEvents.Add(@event);

            EventVersion++;
        }

        public void ApplyEvent(IDomainEvent @event)
        {
            _routeEvents.Handle(@event);

            Version++;
        }

        public void ClearUncommitedEvents()
        {
            _uncommitedEvents.Clear();
        }

        public void LoadFromHistory(IEnumerable<IDomainEvent> events)
        {
            foreach (var @event in events)
            {
                ApplyEvent(@event);
            }
        }
    }
}