using System;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.IntegrationTests.Stubs.DomainLayer;

namespace EnjoyCQRS.IntegrationTests.Stubs.ReadModelLayer
{
    public class FakePersonCreatedEventHandler : IEventHandler<FakePersonCreated>
    {
        public Task ExecuteAsync(FakePersonCreated @event)
        {
            Console.WriteLine(@event);

            return Task.CompletedTask;
        }
    }
}