using System;

namespace MyCQRS.EventStore.Storage
{
    public interface IAggregateCache
    {
        TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate, new();
        void Add<TAggregate>(TAggregate aggregateRoot) where TAggregate : class, IAggregate;
        void Remove(Type aggregateRootType, Guid aggregateRootId);
    }
}