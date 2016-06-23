using System;

namespace EnjoyCQRS.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EventNameAttribute : Attribute
    {
        public string EventName { get; }

        public EventNameAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}