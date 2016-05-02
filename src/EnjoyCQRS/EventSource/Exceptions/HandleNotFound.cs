using System;

namespace EnjoyCQRS.EventSource.Exceptions
{
    public class HandleNotFound : Exception
    {
        public Type EventType { get; }

        public HandleNotFound(Type eventType)
        {
            EventType = eventType;
        }
    }
}