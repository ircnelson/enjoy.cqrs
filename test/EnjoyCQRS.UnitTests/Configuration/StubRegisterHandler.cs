using System;
using System.Collections.Generic;
using EnjoyCQRS.Bus;

namespace EnjoyCQRS.UnitTests.Configuration
{
    public class StubRegisterHandler : IRegisterHandler
    {
        public readonly IDictionary<Type, ICollection<Action<object>>> Routes = new Dictionary<Type, ICollection<Action<object>>>();
        
        public void Register<TMessage>(Action<TMessage> route) where TMessage : class
        {
            var routingKey = typeof(TMessage);
            ICollection<Action<object>> routes;

            if (!Routes.TryGetValue(routingKey, out routes))
                Routes[routingKey] = routes = new LinkedList<Action<object>>();

            routes.Add(message => route(message as TMessage));
        }
    }
}