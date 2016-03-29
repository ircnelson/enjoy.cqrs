using MyCQRS.Events;

namespace MyCQRS.Domain.StubBoundedContext.Events
{
    public class ChangedNameEvent : IEvent
    {
        public string Name { get; }

        public ChangedNameEvent(string name)
        {
            Name = name;
        }
    }
}