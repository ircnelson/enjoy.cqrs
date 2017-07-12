// Copyright (c) 2010-2012, LOKAD SAS
// All rights reserved.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnjoyCQRS.Projections.InMemory
{
    public sealed class MemoryProjectionStore : IProjectionStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>> _store = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>>();
        private readonly IProjectionStrategy _strategy;

        public MemoryProjectionStore(IProjectionStrategy strategy)
        {
            _strategy = strategy;
        }

        public MemoryProjectionStore(IProjectionStrategy strategy, ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>> store) : this (strategy)
        {
            _store = store;
        }

        public IProjectionWriter<TKey, TEntity> GetWriter<TKey, TEntity>()
        {
            var bucket = _strategy.GetEntityBucket<TEntity>();

            var store = _store.GetOrAdd(bucket, s => new ConcurrentDictionary<string, byte[]>());

            return new MemoryProjectionReaderWriter<TKey, TEntity>(_strategy, store);
        }


        public Task ApplyAsync(string bucket, IEnumerable<ProjectionRecord> records)
        {
            var pairs = records.Select(r => new KeyValuePair<string, byte[]>(r.Key, r.Read())).ToArray();

            _store[bucket] = new ConcurrentDictionary<string, byte[]>(pairs);

            return Task.CompletedTask;
        }

        public void ResetAll()
        {
            _store.Clear();
        }

        public void Cleanup(string bucketNames)
        {
            ConcurrentDictionary<string, byte[]> deletedValue;

            _store.TryRemove(bucketNames, out deletedValue);
        }


        public IProjectionReader<TKey, TEntity> GetReader<TKey, TEntity>()
        {
            var bucket = _strategy.GetEntityBucket<TEntity>();

            var store = _store.GetOrAdd(bucket, s => new ConcurrentDictionary<string, byte[]>());

            return new MemoryProjectionReaderWriter<TKey, TEntity>(_strategy, store);
        }

        public IProjectionStrategy Strategy
        {
            get { return _strategy; }
        }

        public Task<IEnumerable<ProjectionRecord>> EnumerateContentsAsync(string bucket)
        {
            var store = _store.GetOrAdd(bucket, s => new ConcurrentDictionary<string, byte[]>());

            var records = store.Select(p => new ProjectionRecord(p.Key, () => p.Value)).ToArray();

            return Task.FromResult<IEnumerable<ProjectionRecord>>(records);
        }

        public string[] GetBuckets()
        {
            return _store.Keys.ToArray();
        }
    }
}
