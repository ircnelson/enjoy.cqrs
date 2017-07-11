// Copyright (c) 2010-2012, LOKAD SAS
// All rights reserved.

using System;

namespace EnjoyCQRS.Projections
{
    public interface IProjectionWriter<in TKey, TEntity>
    {
        TEntity AddOrUpdate(TKey key, Func<TEntity> addValueFactory, Func<TEntity, TEntity> updateValueFactory);
        bool TryDelete(TKey key);
    }
}
