using System;

namespace EnjoyCQRS.EventStore.Storage
{
    /// <summary>
    /// Abstraction of the tracker of the <see ref="IAggregate" />.
    /// </summary>
    public interface IAggregateTracker
    {
        /// <summary>
        /// Retrieves an instance of the <typeparam name="TAggregate"></typeparam> in the tracker based on <param name="id"></param>.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate, new();

        /// <summary>
        /// Add instance of the <typeparam name="TAggregate"></typeparam> to tracking.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="aggregateRoot"></param>
        void Add<TAggregate>(TAggregate aggregateRoot) where TAggregate : class, IAggregate;

        /// <summary>
        /// Remove the aggregate based on <param name="aggregateType"></param> and <param name="aggregateId"></param>.
        /// </summary>
        /// <param name="aggregateType"></param>
        /// <param name="aggregateId"></param>
        void Remove(Type aggregateType, Guid aggregateId);
    }
}