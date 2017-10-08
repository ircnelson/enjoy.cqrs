// Copyright (c) 2010-2012, LOKAD SAS
// All rights reserved.

using System;
using System.Collections.Concurrent;
using System.IO;

namespace EnjoyCQRS.Projections.InMemory
{
    public sealed class MemoryProjectionReaderWriter<TKey, TEntity> : IProjectionReader<TKey, TEntity>, IProjectionWriter<TKey, TEntity>
    {
        private readonly IProjectionStrategy _strategy;
        private readonly ConcurrentDictionary<string, byte[]> _store;

        public MemoryProjectionReaderWriter(IProjectionStrategy strategy, ConcurrentDictionary<string, byte[]> store)
        {
            _store = store;
            _strategy = strategy;
        }

        public bool TryGet(TKey key, out TEntity entity)
        {
            var name = GetName(key);
            byte[] bytes;

            if (_store.TryGetValue(name, out bytes))
            {
                using (var mem = new MemoryStream(bytes))
                {
                    entity = _strategy.Deserialize<TEntity>(mem);
                    return true;
                }
            }

            entity = default(TEntity);

            return false;
        }

        public TEntity AddOrUpdate(TKey key, Func<TEntity> addValueFactory, Func<TEntity, TEntity> updateValueFactory)
        {
            var result = default(TEntity);

            _store.AddOrUpdate(GetName(key), s =>
            {
                result = addValueFactory();

                using (var memory = new MemoryStream())
                {
                    _strategy.Serialize(result, memory);

                    return memory.ToArray();
                }
            }, (s2, bytes) =>
            {
                TEntity entity;

                using (var memory = new MemoryStream(bytes))
                {
                    entity = _strategy.Deserialize<TEntity>(memory);
                }

                result = updateValueFactory(entity);

                using (var memory = new MemoryStream())
                {
                    _strategy.Serialize(result, memory);

                    return memory.ToArray();
                }
            });

            return result;
        }

        public bool TryDelete(TKey key)
        {
            byte[] bytes;

            return _store.TryRemove(GetName(key), out bytes);
        }

        string GetName(TKey key)
        {
            return _strategy.GetEntityLocation<TEntity>(key);
        }
    }
}