using System;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.UnitTests.Shared
{
    public class StubEventRouter : IEventRouter
    {
        private readonly Func<object, Task> _action;

        public static IEventRouter Ok()
        {
            Func<object, Task> route = obj => Task.CompletedTask;

            return new StubEventRouter(route);
        }

        public static IEventRouter Fault()
        {
            Func<object, Task> error = obj =>
            {
                throw new Exception();
            };

            return new StubEventRouter(error);
        }

        public StubEventRouter(Func<object, Task> action = null)
        {
            _action = action;
        }


        public Task RouteAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            return _action(@event);
        }
    }
}