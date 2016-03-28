using System;
using System.Collections.Generic;
using MyCQRS.Events;

namespace MyCQRS.Domain
{
    public class Aggregate
    {
        private readonly List<IEvent> _uncommitedEvents = new List<IEvent>();
        private readonly Dictionary<Type, Action<IEvent>> _routes = new Dictionary<Type, Action<IEvent>>();

        public IReadOnlyCollection<IEvent> UncommitedEvents => _uncommitedEvents.AsReadOnly();
        public int Version { get; protected set; } = -1;

        protected void On<T>(Action<T> action)
            where T : class 
        {
            _routes.Add(typeof(T), o => action(o as T));
        }

        protected void Raise(IEvent @event)
        {
            ApplyEvent(@event);
            _uncommitedEvents.Add(@event);
        }

        public void ApplyEvent(IEvent @event)
        {
            var eventType = @event.GetType();
            if (_routes.ContainsKey(eventType))
            {
                _routes[eventType](@event);
            }

            Version++;
        }

        public void ClearUncommitedEvents()
        {
            _uncommitedEvents.Clear();
        }
    }
}