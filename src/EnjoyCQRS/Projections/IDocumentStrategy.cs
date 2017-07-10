//// Copyright (c) 2010-2012, LOKAD SAS
// All rights reserved.

using System.IO;

namespace EnjoyCQRS.Projections
{
    public interface IDocumentStrategy
    {
        string GetEntityBucket<TEntity>();
        string GetEntityLocation<TEntity>(object key);
        void Serialize<TEntity>(TEntity entity, Stream stream);
        TEntity Deserialize<TEntity>(Stream stream);
    }
}
