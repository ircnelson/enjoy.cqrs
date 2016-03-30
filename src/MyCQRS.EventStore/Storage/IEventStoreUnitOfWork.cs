using System;

namespace MyCQRS.EventStore.Storage
{
    /// <summary>
    /// Represents an abstraction where aggregate events will be persisted.
    /// </summary>
    public interface IEventStoreUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Retrieves an <typeparam name="TAggregate"></typeparam> based on your unique identifier property.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate, new();

        /// <summary>
        /// Add an instance of <typeparam name="TAggregate"></typeparam> in event store.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="aggregate"></param>
        void Add<TAggregate>(TAggregate aggregate) where TAggregate : class, IAggregate, new();
    }
}