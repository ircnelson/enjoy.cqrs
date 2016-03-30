using MyCQRS.EventStore.Storage;
using System;

namespace MyCQRS.EventStore
{
    public class DomainRepository : IDomainRepository
    {
        private readonly IEventStoreUnitOfWork _eventStoreUnitOfWork;
        private readonly IAggregateCache _aggregateCache;

        public DomainRepository(IEventStoreUnitOfWork eventStoreUnitOfWork, IAggregateCache aggregateCache)
        {
            _eventStoreUnitOfWork = eventStoreUnitOfWork;
            _aggregateCache = aggregateCache;
        }

        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate, new()
        {
            _eventStoreUnitOfWork.Add(aggregate);
        }

        public TAggregate GetById<TAggregate>(Guid id)
            where TAggregate : class, IAggregate, new()
        {
            return RegisterForTracking(_aggregateCache.GetById<TAggregate>(id)) ?? _eventStoreUnitOfWork.GetById<TAggregate>(id);
        }

        private TAggregate RegisterForTracking<TAggregate>(TAggregate aggregateRoot) where TAggregate : class, IAggregate, new()
        {
            if (aggregateRoot == null)
                return null;

            _eventStoreUnitOfWork.Add(aggregateRoot);

            return aggregateRoot;
        }
    }
}
