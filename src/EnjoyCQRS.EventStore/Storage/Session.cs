using System;
using System.Collections.Concurrent;
using EnjoyCQRS.Bus;

namespace EnjoyCQRS.EventStore.Storage
{
    public class Session : ISession
    {
        private readonly ConcurrentDictionary<Guid, Aggregate> _aggregateTracker = new ConcurrentDictionary<Guid, Aggregate>();
        private readonly IRepository _repository;
        private readonly IMessageBus _messageBus;

        public Session(IRepository repository, IMessageBus messageBus)
        {
            _repository = repository;
            _messageBus = messageBus;
        }

        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : Aggregate, new()
        {
            TAggregate aggregate;

            if (_aggregateTracker.ContainsKey(id))
            {
                aggregate = (TAggregate) _aggregateTracker[id];

                return aggregate;
            }

            aggregate = _repository.GetById<TAggregate>(id);

            Add(aggregate);

            return aggregate;
        }

        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            if (!_aggregateTracker.ContainsKey(aggregate.Id))
            {
                _aggregateTracker.TryAdd(aggregate.Id, aggregate);
            }
        }

        public void Commit()
        {
            foreach (var aggregate in _aggregateTracker)
            {
                _repository.Save(aggregate.Value);
            }

            _aggregateTracker.Clear();

            _messageBus.Commit();
        }

        public void Rollback()
        {
            _messageBus.Rollback();
        }
    }
}