using System;

namespace EnjoyCQRS.EventSource.Storage
{
    /// <summary>
    /// Represents an abstraction where an instance of <see cref="Aggregate"/> will be persisted.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Retrieves an <typeparam name="TAggregate"></typeparam> based on your unique identifier property.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        TAggregate GetById<TAggregate>(Guid id) where TAggregate : Aggregate, new();

        /// <summary>
        /// Add an instance of <typeparam name="TAggregate"></typeparam> in repository.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="aggregate"></param>
        void Add<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate;
    }
}
