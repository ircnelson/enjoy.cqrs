// Copyright (c) 2010-2012, LOKAD SAS
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnjoyCQRS.Projections
{
    public interface IProjectionStore
    {
        string[] GetBuckets();
        IProjectionWriter<TKey, TEntity> GetWriter<TKey, TEntity>();
        IProjectionReader<TKey, TEntity> GetReader<TKey, TEntity>();
        Task<IEnumerable<ProjectionRecord>> EnumerateContentsAsync(string bucket);
        Task ApplyAsync(string bucket, IEnumerable<ProjectionRecord> records);
        void Cleanup(string bucket);
    }

    public sealed class ProjectionRecord
    {
        public string Key { get; }
        public Func<byte[]> Read { get; }

        public ProjectionRecord(string key, Func<byte[]> read)
        {
            Key = key;
            Read = read;
        }
    }
}
