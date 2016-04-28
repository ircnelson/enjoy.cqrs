using System;
using System.Collections.Generic;
using EnjoyCQRS.EventStore.Exceptions;

namespace EnjoyCQRS.EventStore
{
    public class Route<T> : Dictionary<Type, Action<T>>
    {
        public void Handle(T @event)
        {
            var eventType = @event.GetType();

            if (!ContainsKey(eventType)) throw new HandleNotFound(eventType);

            this[eventType](@event);
        }
    }
}