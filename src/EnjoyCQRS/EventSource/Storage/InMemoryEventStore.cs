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

namespace EnjoyCQRS.EventSource.Storage
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly EnjoyCQRS.Projections.IProjectionStore _projectionStore;
        private readonly ProjectionRebuilder _projectionRebuilder;
        
        private readonly List<ICommittedEvent> _events = new List<ICommittedEvent>();
        private readonly List<ICommittedSnapshot> _snapshots = new List<ICommittedSnapshot>();
        private readonly Dictionary<ProjectionKey, object> _projections = new Dictionary<ProjectionKey, object>();

        private readonly List<IUncommittedEvent> _uncommittedEvents = new List<IUncommittedEvent>();
        private readonly List<IUncommittedSnapshot> _uncommittedSnapshots = new List<IUncommittedSnapshot>();
        private readonly Dictionary<ProjectionKey, object> _uncommittedProjections = new Dictionary<ProjectionKey, object>();

        public IReadOnlyList<ICommittedEvent> Events => _events.AsReadOnly();
        public IReadOnlyList<ICommittedSnapshot> Snapshots => _snapshots.AsReadOnly();
        public IReadOnlyDictionary<ProjectionKey, object> Projections => new ReadOnlyDictionary<ProjectionKey, object>(_projections);

        public InMemoryEventStore(ProjectionRebuilder projectionRebuilder = null)
        {
            _projectionRebuilder = projectionRebuilder;
        }
        
        public bool InTransaction { get; private set; }
        
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
            var events = Events
            .Where(e => e.AggregateId == aggregateId && e.Version > version)
            .OrderBy(e => e.Version)
            .ToList();

            return Task.FromResult<IEnumerable<ICommittedEvent>>(events);
        }

        public void Dispose()
        {
        }

        public void BeginTransaction()
        {
            InTransaction = true;
        }

        public virtual async Task CommitAsync()
        {
            if (!InTransaction) throw new InvalidOperationException("You are not in transaction.");
            
            _events.AddRange(_uncommittedEvents.Select(InstantiateCommittedEvent));

            if (_projectionRebuilder != null)
            {
                await _projectionRebuilder.RebuildAsync(new InMemoryEventStreamReader(_uncommittedEvents)).ConfigureAwait(false);
            }

            _uncommittedEvents.Clear();

            var committedSnapshots = _uncommittedSnapshots.Select(e => new InMemoryCommittedSnapshot(e.AggregateId, e.AggregateVersion, e.Data, e.Metadata));
            
            _snapshots.AddRange(committedSnapshots);
            
            _uncommittedSnapshots.Clear();

            foreach (var uncommittedProjection in _uncommittedProjections.ToList())
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

            _uncommittedProjections.Clear();

            InTransaction = false;

            //return Task.CompletedTask;
        }

        public virtual void Rollback()
        {
            _uncommittedEvents.Clear();
            _uncommittedSnapshots.Clear();
            _uncommittedProjections.Clear();

            InTransaction = false;
        }

        public virtual Task<IEnumerable<ICommittedEvent>> GetAllEventsAsync(Guid id)
        {
            var events = Events
            .Where(e => e.AggregateId == id)
            .OrderBy(e => e.Version)
            .ToList();

            return Task.FromResult<IEnumerable<ICommittedEvent>>(events);
        }

        public virtual Task SaveAsync(IEnumerable<IUncommittedEvent> collection)
        {
            _uncommittedEvents.AddRange(collection);

            return Task.CompletedTask;
        }

        public Task SaveProjectionAsync(IProjection projection)
        {
            var key = new ProjectionKey(projection.Id, projection.GetType().Name);

            if (!_uncommittedProjections.ContainsKey(key))
            {
                _uncommittedProjections.Add(key, null);
            }

            _uncommittedProjections[key] = projection;

            return Task.CompletedTask;
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