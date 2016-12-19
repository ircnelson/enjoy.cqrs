using System.Reflection;
using EnjoyCQRS.Attributes;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Extensions
{
    public static class DomainEventExtensions
    {
        public static bool TryGetEventNameAttribute(this IDomainEvent @event, out string eventName)
        {

            EventNameAttribute attribute;

#if REFLECTIONBRIDGE && (!(NET40 || NET35 || NET20))
            attribute =  @event.GetType().GetCustomAttribute<EventNameAttribute>();
#else
            attribute = @event.GetType().GetTypeInfo().GetCustomAttribute<EventNameAttribute>();
#endif
            eventName = attribute?.EventName;
            
            return !string.IsNullOrWhiteSpace(eventName);
        }
    }
}