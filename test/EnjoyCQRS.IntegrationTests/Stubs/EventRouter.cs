using System.Collections.Generic;
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
        
        public void Route<TEvent>(TEvent @event) 
            where TEvent : IDomainEvent
        {
            var handlers = _scope.ResolveOptional<IEnumerable<IEventHandler<TEvent>>>();

            foreach (var handler in handlers)
            {
                handler.Execute(@event);
            }
        }
    }
}