using System;
using System.Linq;
using EnjoyCQRS.Bus;

namespace EnjoyCQRS.EventStore.Storage
{
    public class Repository : IRepository
    {
        private readonly IEventStore _eventStore;
        private readonly IMessageBus _messageBus;

        public Repository(IEventStore eventStore, IMessageBus messageBus)
        {
            _eventStore = eventStore;
            _messageBus = messageBus;
        }

        public TAggregate GetById<TAggregate>(Guid id)
            where TAggregate : Aggregate, new()
        {
            var aggregate = new TAggregate();
            var events = _eventStore.GetAllEvents(id);

            aggregate.LoadFromHistory(events);

            return aggregate;
        }

        public void Save<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            var changes = aggregate.UncommitedEvents.ToList();
            aggregate.ClearUncommitedEvents();

            _eventStore.Save(changes);

            _messageBus.Publish(changes.Select(e => (object)e));
        }
    }
}
