using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Infrastructure
{
    public class EventUpdateManager : IEventUpdateManager
    {
        private readonly ILifetimeScope _scope;
        private readonly Dictionary<Type, object> _eventUpdate = new Dictionary<Type, object>();

        public EventUpdateManager(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public IEnumerable<IDomainEvent> Update(IEnumerable<IDomainEvent> events)
        {
            var eventUpdateTypeOf = typeof(IEventUpdate<>);

            var updatedEvents = new List<IDomainEvent>();

            foreach (var @event in events)
            {
                var eventType = @event.GetType();

                object eventUpdate;

                if (!_eventUpdate.ContainsKey(eventType))
                {
                    eventUpdate = _scope.ResolveOptional(eventUpdateTypeOf.MakeGenericType(eventType));

                    if (eventUpdate == null)
                    {
                        updatedEvents.Add(@event);

                        continue;
                    }

                    _eventUpdate.Add(eventType, eventUpdate);
                }

                eventUpdate = _eventUpdate[eventType];

                var methodInfo = eventUpdate.GetType().GetMethod(nameof(IEventUpdate<object>.Update), BindingFlags.Instance | BindingFlags.Public);
                var resultMethod = (IEnumerable<IDomainEvent>) methodInfo.Invoke(eventUpdate, new object[] {@event});

                updatedEvents.AddRange(resultMethod);
            }

            return updatedEvents;
        }
    }
}
