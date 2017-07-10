// Copyright (c) 2010-2012, LOKAD SAS
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnjoyCQRS.Projections
{
    public interface IDocumentStore
    {
        string[] GetBuckets();
        IDocumentWriter<TKey, TEntity> GetWriter<TKey, TEntity>();
        IDocumentReader<TKey, TEntity> GetReader<TKey, TEntity>();
        Task<IEnumerable<DocumentRecord>> EnumerateContentsAsync(string bucket);
        Task ApplyAsync(string bucket, IEnumerable<DocumentRecord> records);
        void Cleanup(string bucket);
    }

    public sealed class DocumentRecord
    {
        public string Key { get; }
        public Func<byte[]> Read { get; }

        public DocumentRecord(string key, Func<byte[]> read)
        {
            Key = key;
            Read = read;
        }
    }
}
