using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.FooAggregate;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.EventHandlers
{
    public class FooCreatedHandler : IEventHandler<FooCreated>
    {
        public Task ExecuteAsync(FooCreated @event)
        {
            return Task.CompletedTask;
        }
    }
}
