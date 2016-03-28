using MyCQRS.Events;

namespace MyCQRS.Domain.Specs.Stubs
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