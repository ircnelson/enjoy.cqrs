// Copyright (c) 2010-2012, LOKAD SAS
// All rights reserved.

using System;

namespace EnjoyCQRS.Projections
{
    public static class DocumentWriterExtensions
    {
        public static TEntity AddOrUpdate<TKey, TEntity>(this IProjectionWriter<TKey, TEntity> writer, TKey key, Func<TEntity> addFactory, Action<TEntity> update)
        {
            return writer.AddOrUpdate(key, addFactory, entity =>
            {
                update(entity);
                return entity;
            });
        }

        public static TEntity AddOrUpdate<TKey, TEntity>(this IProjectionWriter<TKey, TEntity> writer, TKey key, TEntity newView, Action<TEntity> updateViewFactory)
        {
            return writer.AddOrUpdate(key, () => newView, view =>
            {
                updateViewFactory(view);
                return view;
            });
        }

        public static TEntity Add<TKey, TEntity>(this IProjectionWriter<TKey, TEntity> writer, TKey key, TEntity newEntity)
        {
            return writer.AddOrUpdate(key, newEntity, e =>
            {
                var txt = $"Entity '{typeof(TEntity).Name}' with key '{key}' should not exist.";
                throw new InvalidOperationException(txt);
            });
        }

        public static TEntity UpdateOrThrow<TKey, TEntity>(this IProjectionWriter<TKey, TEntity> writer, TKey key, Func<TEntity, TEntity> change)
        {
            return writer.AddOrUpdate(key, () =>
            {
                var txt = $"Failed to load '{typeof(TEntity).Name}' with key '{key}'.";
                throw new InvalidOperationException(txt);
            }, change);
        }

        public static TEntity UpdateOrThrow<TKey, TEntity>(this IProjectionWriter<TKey, TEntity> writer, TKey key, Action<TEntity> change)
        {
            return writer.AddOrUpdate(key, () =>
            {
                var txt = $"Failed to load '{typeof(TEntity).Name}' with key '{key}'.";
                throw new InvalidOperationException(txt);
            }, change);
        }

        public static TView UpdateEnforcingNew<TKey, TView>(this IProjectionWriter<TKey, TView> writer, TKey key,
            Action<TView> update)
            where TView : new()
        {
            return writer.AddOrUpdate(key, () =>
            {
                var view = new TView();
                update(view);
                return view;
            }, v =>
            {
                update(v);
                return v;
            });
        }
    }
}
