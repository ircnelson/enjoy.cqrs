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

using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Snapshots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnjoyCQRS.Stores.InMemory
{
    public class InMemorySnapshotStore : ISnapshotStore
    {
        private readonly List<ICommittedSnapshot> _snapshots;
        private readonly List<IUncommittedSnapshot> _uncommittedSnapshots = new List<IUncommittedSnapshot>();
        private readonly InMemoryEventStore _eventStore;

        public IReadOnlyList<ICommittedSnapshot> Snapshots => _snapshots.AsReadOnly();

        public IReadOnlyList<IUncommittedSnapshot> Uncommitted => _uncommittedSnapshots.AsReadOnly();

        public InMemorySnapshotStore(InMemoryEventStore eventStore, List<ICommittedSnapshot> storage)
        {
            _eventStore = eventStore;
            _snapshots = storage;
        }

        public virtual Task SaveSnapshotAsync(IUncommittedSnapshot snapshot)
        {
            _uncommittedSnapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public virtual Task<ICommittedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var snapshot = Snapshots.Where(e => e.AggregateId == aggregateId).OrderByDescending(e => e.AggregateVersion).Take(1).FirstOrDefault();

            return Task.FromResult(snapshot);
        }

        public virtual Task<IEnumerable<ICommittedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            return _eventStore.GetEventsForwardAsync(aggregateId, version);
        }

        internal void ClearUncommitted()
        {
            _uncommittedSnapshots.Clear();
        }

        public void Dispose()
        {
            ClearUncommitted();
        }
    }
}
