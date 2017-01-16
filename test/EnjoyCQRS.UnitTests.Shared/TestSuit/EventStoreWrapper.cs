using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.UnitTests.Shared.TestSuit
{
    public class EventStoreWrapper : IEventStore
    {
        public EventStoreMethods CalledMethods;

        private readonly IEventStore _eventStore;

        public EventStoreWrapper(IEventStore eventStore)
        {
            _eventStore = eventStore;

            CalledMethods &= EventStoreMethods.Ctor;
        }

        public async Task SaveSnapshotAsync(ISerializedSnapshot snapshot)
        {
            await _eventStore.SaveSnapshotAsync(snapshot).ConfigureAwait(false);

            CalledMethods |= EventStoreMethods.SaveSnapshotAsync;
        }

        public async Task<ICommitedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var result = await _eventStore.GetLatestSnapshotByIdAsync(aggregateId).ConfigureAwait(false);

            CalledMethods |= EventStoreMethods.GetLatestSnapshotByIdAsync;

            return result;
        }

        public async Task<IEnumerable<ICommitedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var result = await _eventStore.GetEventsForwardAsync(aggregateId, version).ConfigureAwait(false);

            CalledMethods |= EventStoreMethods.GetEventsForwardAsync;

            return result;
        }

        public void Dispose()
        {
            _eventStore.Dispose();

            CalledMethods |= EventStoreMethods.Dispose;
        }

        public void BeginTransaction()
        {
            _eventStore.BeginTransaction();

            CalledMethods |= EventStoreMethods.BeginTransaction;
        }

        public async Task CommitAsync()
        {
            await _eventStore.CommitAsync();

            CalledMethods |= EventStoreMethods.CommitAsync;
        }

        public void Rollback()
        {
            _eventStore.Rollback();

            CalledMethods |= EventStoreMethods.Rollback;
        }

        public async Task<IEnumerable<ICommitedEvent>> GetAllEventsAsync(Guid id)
        {
            var result = await _eventStore.GetAllEventsAsync(id).ConfigureAwait(false);

            CalledMethods |= EventStoreMethods.GetAllEventsAsync;

            return result;
        }

        public async Task SaveAsync(IEnumerable<ISerializedEvent> collection)
        {
            await _eventStore.SaveAsync(collection).ConfigureAwait(false);

            CalledMethods |= EventStoreMethods.SaveAsync;
        }

        public async Task SaveProjectionAsync(IProjection projection)
        {
            await _eventStore.SaveProjectionAsync(projection);

            CalledMethods |= EventStoreMethods.SaveAggregateProjection;
        }
    }
}