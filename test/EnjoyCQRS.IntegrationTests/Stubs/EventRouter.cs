using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        public Task RouteAsync<TEvent>(TEvent @event) 
            where TEvent : IDomainEvent
        {
            var handlers = _scope.ResolveOptional<IEnumerable<IEventHandler<TEvent>>>();

            foreach (var handler in handlers)
            {
                handler.ExecuteAsync(@event);
            }

            return Task.CompletedTask;
        }
    }
}