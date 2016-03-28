using System;
using System.Collections.Generic;

namespace MyCQRS.Domain
{
    public class Route<T> : Dictionary<Type, Action<T>>
    {
        public void Handle(T @event)
        {
            var eventType = @event.GetType();

            if (!ContainsKey(eventType)) throw new Exception();

            this[eventType](@event);
        }
    }
}