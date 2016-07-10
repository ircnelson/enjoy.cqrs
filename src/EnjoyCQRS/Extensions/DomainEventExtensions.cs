using System.Reflection;
using EnjoyCQRS.Attributes;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Extensions
{
    public static class DomainEventExtensions
    {
        public static bool TryGetEventNameAttribute(this IDomainEvent @event, out string eventName)
        {
            var attribute = @event.GetType().GetCustomAttribute<EventNameAttribute>();

            eventName = attribute?.EventName;
            
            return !string.IsNullOrWhiteSpace(eventName);
        }
    }
}