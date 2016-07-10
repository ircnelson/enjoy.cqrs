using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FooAggregate;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.EventHandlers
{
    public class FooCreatedHandler : IEventHandler<FooCreated>
    {
        public Task ExecuteAsync(FooCreated @event)
        {
            return Task.CompletedTask;
        }
    }
}
