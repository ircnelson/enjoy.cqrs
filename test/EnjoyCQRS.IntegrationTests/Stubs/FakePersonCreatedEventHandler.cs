using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class FakePersonCreatedEventHandler : IEventHandler<FakePersonCreatedEvent>
    {
        public void Execute(FakePersonCreatedEvent @event)
        {
            Console.WriteLine(@event);
        }
    }
}