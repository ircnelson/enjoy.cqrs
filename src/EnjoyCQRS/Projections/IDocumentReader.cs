// Copyright (c) 2010-2012, LOKAD SAS
// All rights reserved.

namespace EnjoyCQRS.Projections
{
    public interface IDocumentReader<in TKey, TView>
    {
        bool TryGet(TKey key, out TView view);
    }
}
