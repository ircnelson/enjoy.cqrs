using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.MessageBus.Stubs
{
    public class OrderTestEventHandler : IEventHandler<OrderedTestEvent>
    {
        private readonly List<Tuple<Guid, int>> _eventsBag;

        public OrderTestEventHandler(List<Tuple<Guid, int>> eventsBag)
        {
            _eventsBag = eventsBag;
        }

        public Task ExecuteAsync(OrderedTestEvent @event)
        {
            _eventsBag.Add(new Tuple<Guid, int>(@event.AggregateId, @event.Order));

            return Task.CompletedTask;
        }
    }
}