using MyCQRS.Events;

namespace MyCQRS.UnitTests.Domain.Events
{
    public class SomeEvent : DomainEvent
    {
        public string Name { get; }

        public SomeEvent(string name)
        {
            Name = name;
        }
    }
}