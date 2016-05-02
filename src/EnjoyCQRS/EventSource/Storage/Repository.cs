using System;

namespace EnjoyCQRS.EventSource.Storage
{
    public class Repository : IRepository
    {
        private readonly ISession _session;
        private readonly IAggregateTracker _aggregateTracker;

        public Repository(ISession session, IAggregateTracker aggregateTracker)
        {
            _session = session;
            _aggregateTracker = aggregateTracker;
        }

        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            _session.Add(aggregate);
        }

        public TAggregate GetById<TAggregate>(Guid id)
            where TAggregate : Aggregate, new()
        {
            return RegisterForTracking(_aggregateTracker.GetById<TAggregate>(id)) ?? _session.GetById<TAggregate>(id);
        }

        private TAggregate RegisterForTracking<TAggregate>(TAggregate aggregateRoot) where TAggregate : Aggregate, new()
        {
            if (aggregateRoot == null)
                return null;

            _session.Add(aggregateRoot);

            return aggregateRoot;
        }
    }
}
