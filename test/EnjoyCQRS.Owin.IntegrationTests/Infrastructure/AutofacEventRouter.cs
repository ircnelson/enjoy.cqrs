using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using EnjoyCQRS.Events;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.Owin.IntegrationTests.Infrastructure
{
    public class AutofacEventRouter : IEventRouter
    {
        private readonly ILifetimeScope _scope;

        public AutofacEventRouter(ILifetimeScope scope)
        {
            _scope = scope;
        }
        
        public async Task RouteAsync<TEvent>(TEvent @event) 
            where TEvent : IDomainEvent
        {
            var handlers = _scope.ResolveOptional<IEnumerable<IEventHandler<TEvent>>>();

            foreach (var handler in handlers)
            {
                await handler.ExecuteAsync(@event).ConfigureAwait(false);
            }
        }
    }
}