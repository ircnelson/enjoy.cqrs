using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MyCQRS.EventStore.Storage
{
    public class AggregateCache : IAggregateCache
    {
        private readonly ConcurrentDictionary<Type, Dictionary<Guid, object>> _cache = new ConcurrentDictionary<Type, Dictionary<Guid, object>>();

        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate, new()
        {
            Dictionary<Guid, object> aggregates;
            if (!_cache.TryGetValue(typeof(TAggregate), out aggregates))
                return null;

            object aggregate;
            if (!aggregates.TryGetValue(id, out aggregate))
                return null;

            return (TAggregate)aggregate;
        }

        public void Add<TAggregate>(TAggregate aggregateRoot) where TAggregate : class, IAggregate
        {
            Dictionary<Guid, object> aggregates;
            if (!_cache.TryGetValue(typeof(TAggregate), out aggregates))
            {
                aggregates = new Dictionary<Guid, object>();
                _cache.TryAdd(typeof(TAggregate), aggregates);
            }

            if (aggregates.ContainsKey(aggregateRoot.Id))
                return;

            aggregates.Add(aggregateRoot.Id, aggregateRoot);
        }

        public void Remove(Type aggregateType, Guid aggregateId)
        {
            Dictionary<Guid, object> aggregates;
            if (!_cache.TryGetValue(aggregateType, out aggregates))
                return;

            aggregates.Remove(aggregateId);
        }
    }
}