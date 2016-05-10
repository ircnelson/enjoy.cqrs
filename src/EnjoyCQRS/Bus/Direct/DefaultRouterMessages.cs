using System;
using System.Collections.Generic;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Bus.Direct
{
    public class DefaultRouterMessages : ICommandRouter, IEventRouter, IRegisterHandler
    {
        private readonly IDictionary<Type, ICollection<Action<object>>> _routes = new Dictionary<Type, ICollection<Action<object>>>();
        
        public void Register<TMessage>(Action<TMessage> route) where TMessage : class
        {
            var routingKey = typeof(TMessage);
            ICollection<Action<object>> routes;

            if (!_routes.TryGetValue(routingKey, out routes))
                _routes[routingKey] = routes = new LinkedList<Action<object>>();

            routes.Add(message => route(message as TMessage));
        }

        private void DoRoute(object message)
        {
            ICollection<Action<object>> routes;

            if (!_routes.TryGetValue(message.GetType(), out routes))
                throw new RouteNotRegisteredException(message.GetType());

            foreach (var route in routes)
                route(message);
        }

        public void Route(ICommand command)
        {
            DoRoute(command);
        }

        public void Route(IDomainEvent @event)
        {
            DoRoute(@event);
        }
    }
}