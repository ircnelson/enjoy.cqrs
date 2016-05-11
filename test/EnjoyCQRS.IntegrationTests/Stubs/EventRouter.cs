using Autofac;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class EventRouter : IEventRouter
    {
        private readonly ILifetimeScope _scope;

        public EventRouter(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public void Route(IDomainEvent @event)
        {
        }
    }
}