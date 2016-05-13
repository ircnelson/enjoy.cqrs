using System;
using System.Threading.Tasks;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class FakePersonCreatedEventHandler : IEventHandler<FakePersonCreatedEvent>
    {
        public Task ExecuteAsync(FakePersonCreatedEvent @event)
        {
            Console.WriteLine(@event);

            return Task.CompletedTask;
        }
    }
}