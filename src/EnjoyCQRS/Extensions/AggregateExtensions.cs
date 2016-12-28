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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.Extensions
{
    internal static class AggregateExtensions
    {
        public static async Task TakeSnapshot(this Aggregate aggregate,
            IEventStore eventStore,
            ISnapshotSerializer snapshotSerializer)
        {
            var snapshot = ((ISnapshotAggregate)aggregate).CreateSnapshot();

            var metadatas = new[]
            {
                new KeyValuePair<string, object>(MetadataKeys.AggregateId, aggregate.Id),
                new KeyValuePair<string, object>(MetadataKeys.AggregateSequenceNumber, aggregate.Sequence),
                new KeyValuePair<string, object>(MetadataKeys.SnapshotId, Guid.NewGuid()),
                new KeyValuePair<string, object>(MetadataKeys.SnapshotClrType, snapshot.GetType().AssemblyQualifiedName),
                new KeyValuePair<string, object>(MetadataKeys.SnapshotName, snapshot.GetType().Name),
            };

            var serializedSnapshot = snapshotSerializer.Serialize(aggregate, snapshot, metadatas);

            await eventStore.SaveSnapshotAsync(serializedSnapshot).ConfigureAwait(false);
        }
    }
}