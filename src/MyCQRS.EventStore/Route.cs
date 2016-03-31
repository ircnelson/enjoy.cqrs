using System;
using System.Collections.Generic;
using MyCQRS.EventStore.Exceptions;

namespace MyCQRS.EventStore
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