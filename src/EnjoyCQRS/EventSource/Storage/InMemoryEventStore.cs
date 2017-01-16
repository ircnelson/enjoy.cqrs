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

namespace EnjoyCQRS.EventSource.Storage
{
    public class InMemoryEventStore : IEventStore
    {
        public IReadOnlyList<ICommitedEvent> Events => _events.AsReadOnly();
        public IReadOnlyList<ICommitedSnapshot> Snapshots => _snapshots.AsReadOnly();
        public IReadOnlyDictionary<ProjectionKey, object> Projections => new ReadOnlyDictionary<ProjectionKey, object>(_projections);

        private readonly List<ICommitedEvent> _events = new List<ICommitedEvent>();
        private readonly List<ICommitedSnapshot> _snapshots = new List<ICommitedSnapshot>();
        private readonly Dictionary<ProjectionKey, object> _projections = new Dictionary<ProjectionKey, object>();

        private readonly List<ISerializedEvent> _uncommitedEvents = new List<ISerializedEvent>();
        private readonly List<ISerializedSnapshot> _uncommitedSnapshots = new List<ISerializedSnapshot>();
        private readonly Dictionary<ProjectionKey, object> _uncommitedProjections = new Dictionary<ProjectionKey, object>();

        public bool InTransaction;
        
        public virtual Task SaveSnapshotAsync(ISerializedSnapshot snapshot)
        {
            _uncommitedSnapshots.Add(snapshot);

            return Task.CompletedTask;
        }

        public virtual Task<ICommitedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var snapshot = Snapshots.Where(e => e.AggregateId == aggregateId).OrderByDescending(e => e.AggregateVersion).Take(1).FirstOrDefault();

            return Task.FromResult(snapshot);
        }

        public virtual Task<IEnumerable<ICommitedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var events = Events
            .Where(e => e.AggregateId == aggregateId && e.AggregateVersion > version)
            .OrderBy(e => e.AggregateVersion)
            .ToList();

            return Task.FromResult<IEnumerable<ICommitedEvent>>(events);
        }

        public void Dispose()
        {
        }

        public void BeginTransaction()
        {
            InTransaction = true;
        }

        public virtual Task CommitAsync()
        {
            if (!InTransaction) throw new InvalidOperationException("You are not in transaction.");

            InTransaction = false;

            _events.AddRange(_uncommitedEvents.Select(InstantiateCommitedEvent));

            _uncommitedEvents.Clear();

            var commitedSnapshots = _uncommitedSnapshots.Select(e => new InMemoryCommitedSnapshot(e.AggregateId, e.AggregateVersion, e.SerializedData, e.SerializedMetadata));
            
            _snapshots.AddRange(commitedSnapshots);
            
            _uncommitedSnapshots.Clear();

            foreach (var uncommitedProjection in _uncommitedProjections.ToList())
            {
                if (!_projections.ContainsKey(uncommitedProjection.Key))
                {
                    _projections.Add(uncommitedProjection.Key, uncommitedProjection.Value);
                }
                else
                {
                    _projections[uncommitedProjection.Key] = uncommitedProjection.Value;
                }
            }

            _uncommitedProjections.Clear();

            return Task.CompletedTask;
        }

        public virtual void Rollback()
        {
            _uncommitedEvents.Clear();
            _uncommitedSnapshots.Clear();
            _uncommitedProjections.Clear();

            InTransaction = false;
        }

        public virtual Task<IEnumerable<ICommitedEvent>> GetAllEventsAsync(Guid id)
        {
            var events = Events
            .Where(e => e.AggregateId == id)
            .OrderBy(e => e.AggregateVersion)
            .ToList();

            return Task.FromResult<IEnumerable<ICommitedEvent>>(events);
        }

        public virtual Task SaveAsync(IEnumerable<ISerializedEvent> collection)
        {
            _uncommitedEvents.AddRange(collection);

            return Task.CompletedTask;
        }

        public Task SaveProjectionAsync(IProjection projection)
        {
            var key = new ProjectionKey(projection.Id, projection.Category);

            if (!_uncommitedProjections.ContainsKey(key))
            {
                _uncommitedProjections.Add(key, null);
            }

            _uncommitedProjections[key] = projection.Projection;

            return Task.CompletedTask;
        }

        private static ICommitedEvent InstantiateCommitedEvent(ISerializedEvent serializedEvent)
        {
            return new InMemoryCommitedEvent(serializedEvent.AggregateId, serializedEvent.AggregateVersion, serializedEvent.SerializedData, serializedEvent.SerializedMetadata);
        }

        internal class InMemoryCommitedEvent : ICommitedEvent
        {
            public InMemoryCommitedEvent(Guid aggregateId, int aggregateVersion, string serializedData, string serializedMetadata)
            {
                AggregateId = aggregateId;
                AggregateVersion = aggregateVersion;
                SerializedData = serializedData;
                SerializedMetadata = serializedMetadata;
            }

            public Guid AggregateId { get; }
            public int AggregateVersion { get; }
            public string SerializedData { get; }
            public string SerializedMetadata { get; }
        }

        internal class InMemoryCommitedSnapshot : ICommitedSnapshot
        {
            public InMemoryCommitedSnapshot(Guid aggregateId, int aggregateVersion, string serializedData, string serializedMetadata)
            {
                AggregateId = aggregateId;
                AggregateVersion = aggregateVersion;
                SerializedData = serializedData;
                SerializedMetadata = serializedMetadata;
            }

            public Guid AggregateId { get; }
            public int AggregateVersion { get; }
            public string SerializedData { get; }
            public string SerializedMetadata { get; }
        }

        public struct ProjectionKey
        {
            public Guid Id { get; }
            public string Category { get;}

            public ProjectionKey(Guid id, string category)
            {
                Id = id;
                Category = category;
            }
        }
    }
}