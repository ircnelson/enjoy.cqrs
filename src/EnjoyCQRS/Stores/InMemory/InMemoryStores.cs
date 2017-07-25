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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Projections;
using System.Threading;
using EnjoyCQRS.Stores;
using EnjoyCQRS.Core;
using IProjectionStoreV1 = EnjoyCQRS.EventSource.Projections.IProjectionStore;

namespace EnjoyCQRS.Stores.InMemory
{
    public class InMemoryStores :  ITransaction, ICompositeStores
    {
        private readonly IProjectionStoreV1 _projectionStore;
        private readonly ProjectionRebuilder _projectionRebuilder;
        
        private readonly List<ICommittedEvent> _events = new List<ICommittedEvent>();
        private readonly List<ICommittedSnapshot> _snapshots = new List<ICommittedSnapshot>();
        private readonly Dictionary<ProjectionKey, object> _projections = new Dictionary<ProjectionKey, object>();

        private readonly InMemoryEventStore _eventStore;
        private readonly InMemorySnapshotStore _snapshotStore;
        private readonly InMemoryProjectionStoreV1 _projectionStoreV1;

        public IReadOnlyList<ICommittedEvent> Events => _events.AsReadOnly();
        public IReadOnlyList<ICommittedSnapshot> Snapshots => _snapshots.AsReadOnly();
        public IReadOnlyDictionary<ProjectionKey, object> Projections => new ReadOnlyDictionary<ProjectionKey, object>(_projections);
        public bool InTransaction { get; private set; }
        public IEventStore EventStore => _eventStore;
        public ISnapshotStore SnapshotStore => _snapshotStore;
        public IProjectionStoreV1 ProjectionStoreV1 => _projectionStoreV1;

        public InMemoryStores(ProjectionRebuilder projectionRebuilder = null)
        {
            _eventStore = new InMemoryEventStore(_events);
            _snapshotStore = new InMemorySnapshotStore(_eventStore, _snapshots);
            _projectionStoreV1 = new InMemoryProjectionStoreV1(_projections);
            
            _projectionRebuilder = projectionRebuilder;
        }
        
        public void BeginTransaction()
        {
            InTransaction = true;
        }

        public virtual async Task CommitAsync()
        {
            if (!InTransaction) throw new InvalidOperationException("You are not in transaction.");

            var uncommittedEvents = _eventStore.Uncommitted.ToList();

            _events.AddRange(uncommittedEvents.Select(InstantiateCommittedEvent));

            if (_projectionRebuilder != null)
            {
                await _projectionRebuilder.RebuildAsync(new InMemoryEventStreamReader(uncommittedEvents)).ConfigureAwait(false);
            }

            _eventStore.ClearUncommitted();

            var uncommittedSnapshots = _snapshotStore.Uncommitted.ToList();

            var committedSnapshots = uncommittedSnapshots.Select(e => new InMemoryCommittedSnapshot(e.AggregateId, e.AggregateVersion, e.Data, e.Metadata));

            _snapshots.AddRange(committedSnapshots);

            _snapshotStore.ClearUncommitted();

            var uncommittedProjections = _projectionStoreV1.Uncommitted.ToList();

            foreach (var uncommittedProjection in uncommittedProjections)
            {
                if (!_projections.ContainsKey(uncommittedProjection.Key))
                {
                    _projections.Add(uncommittedProjection.Key, uncommittedProjection.Value);
                }
                else
                {
                    _projections[uncommittedProjection.Key] = uncommittedProjection.Value;
                }
            }

            _projectionStoreV1.ClearUncommitted();

            InTransaction = false;

            //return Task.CompletedTask;
        }

        public virtual void Rollback()
        {
            _eventStore.ClearUncommitted();
            _snapshotStore.ClearUncommitted();
            _projectionStoreV1.ClearUncommitted();

            InTransaction = false;
        }
        
        private static ICommittedEvent InstantiateCommittedEvent(IUncommittedEvent serializedEvent)
        {
            return new InMemoryCommittedEvent(serializedEvent.AggregateId, serializedEvent.Version, serializedEvent.Data, serializedEvent.Metadata);
        }

        internal class InMemoryCommittedEvent : ICommittedEvent
        {
            public Guid AggregateId { get; }
            public int Version { get; }
            public object Data { get; }
            public IMetadataCollection Metadata { get; }

            public InMemoryCommittedEvent(Guid aggregateId, int aggregateVersion, object data, IMetadataCollection metadata)
            {
                AggregateId = aggregateId;
                Version = aggregateVersion;
                Data = data;
                Metadata = metadata;
            }

        }

        internal class InMemoryCommittedSnapshot : ICommittedSnapshot
        {
            public InMemoryCommittedSnapshot(Guid aggregateId, int aggregateVersion, ISnapshot serializedData, IMetadataCollection serializedMetadata)
            {
                AggregateId = aggregateId;
                AggregateVersion = aggregateVersion;
                Data = serializedData;
                Metadata = serializedMetadata;
            }

            public Guid AggregateId { get; }
            public int AggregateVersion { get; }
            public ISnapshot Data { get; }
            public IMetadataCollection Metadata { get; }
        }

        public void Dispose()
        {
        }
    }    

    public struct ProjectionKey
    {
        public Guid Id { get; }
        public string Category { get; }

        public ProjectionKey(Guid id, string category)
        {
            Id = id;
            Category = category;
        }
    }

    public class InMemoryEventStreamReader : EventStreamReader
    {
        private readonly List<IUncommittedEvent> _uncommittedEvents;

        public InMemoryEventStreamReader(List<IUncommittedEvent> uncommittedEvents)
        {
            _uncommittedEvents = uncommittedEvents;
        }

        public override bool DeleteAllRecords => false;

        public override Task ReadAsync(CancellationToken cancellationToken, OnDeserializeEventDelegate onDeserializeEvent)
        {
            foreach (var uncommittedEvent in _uncommittedEvents)
            {
                onDeserializeEvent(uncommittedEvent.Data, uncommittedEvent.Metadata);
            }

            return Task.CompletedTask;
        }
    }
}