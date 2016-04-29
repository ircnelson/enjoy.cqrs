using System;
using EnjoyCQRS.EventStore.Storage;

namespace EnjoyCQRS.EventStore
{
    public class DomainRepository : IDomainRepository
    {
        private readonly ISession _session;
        private readonly IAggregateTracker _aggregateCache;

        public DomainRepository(ISession session, IAggregateTracker aggregateCache)
        {
            _session = session;
            _aggregateCache = aggregateCache;
        }

        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate, new()
        {
            _session.Add(aggregate);
        }

        public TAggregate GetById<TAggregate>(Guid id)
            where TAggregate : class, IAggregate, new()
        {
            return RegisterForTracking(_aggregateCache.GetById<TAggregate>(id)) ?? _session.GetById<TAggregate>(id);
        }

        private TAggregate RegisterForTracking<TAggregate>(TAggregate aggregateRoot) where TAggregate : class, IAggregate, new()
        {
            if (aggregateRoot == null)
                return null;

            _session.Add(aggregateRoot);

            return aggregateRoot;
        }
    }
}
