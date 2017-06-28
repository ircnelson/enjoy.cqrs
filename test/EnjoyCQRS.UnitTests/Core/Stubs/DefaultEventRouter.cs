using EnjoyCQRS.Events;
using EnjoyCQRS.MessageBus;
using System.Threading.Tasks;

namespace EnjoyCQRS.UnitTests.Core.Stubs
{
    class DefaultEventRouter : IEventRouter
    {
        public Task RouteAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            return Task.CompletedTask;
        }
    }
}
