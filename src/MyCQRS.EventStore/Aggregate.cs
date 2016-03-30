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
        public Guid Id { get; protected set; }
        public int Version { get; protected set; } = -1;

        protected void On<T>(Action<T> action)
            where T : class 
        {
            _routeEvents.Add(typeof(T), o => action(o as T));
        }

        protected void Raise(IDomainEvent @event)
        {
            ApplyEvent(@event);
            _uncommitedEvents.Add(@event);
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

        public void LoadFromHistory(IEnumerable<IDomainEvent> enumerable)
        {
            foreach (var @event in enumerable)
            {
                ApplyEvent(@event);
            }
        }
    }
}