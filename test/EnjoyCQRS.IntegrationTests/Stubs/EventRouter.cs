using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
            var genericHandler = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
            var enumerableGenericHandler = typeof(IEnumerable<>).MakeGenericType(genericHandler);
            var handlers = _scope.ResolveOptional(enumerableGenericHandler) as IEnumerable;

            foreach (var handler in handlers)
            {
                var methodInfo = handler.GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public);
                methodInfo.Invoke(handler, new[] { @event });
            }
        }
    }
}