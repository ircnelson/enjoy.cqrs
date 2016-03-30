using System;

namespace MyCQRS.EventStore.Storage
{
    public interface IEventStoreUnitOfWork : IUnitOfWork
    {
        TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate, new();
        void Add<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate;
    }
}