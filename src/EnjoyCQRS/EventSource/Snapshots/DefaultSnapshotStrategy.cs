// The MIT License (MIT)
// 
// Copyright (c) 2016 Nelson Corrêa V. Júnior
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Reflection;

namespace EnjoyCQRS.EventSource.Snapshots
{
    public class DefaultSnapshotStrategy : ISnapshotStrategy
    {
        private static readonly Type SnapshotType = typeof (ISnapshotAggregate);
        
        public bool CheckSnapshotSupport(Type aggregateType)
        {
            Type baseType;

#if REFLECTIONBRIDGE && (!(NET40 || NET35 || NET20))
            baseType = aggregateType.BaseType;
#else
            baseType = aggregateType.GetTypeInfo().BaseType;
#endif

            if (baseType == null) return false;

            if (SnapshotType.IsAssignableFrom(aggregateType)) return true;

            return CheckSnapshotSupport(baseType);
        }

        public virtual bool ShouldMakeSnapshot(IAggregate aggregate)
        {
            if (!CheckSnapshotSupport(aggregate.GetType())) return false;

            return true;
        }
    }
}