using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.Stores;
using EnjoyCQRS.Core;

namespace EnjoyCQRS.UnitTests.Shared.TestSuit
{
    public class StoresWrapper : ITransaction, ICompositeStores
    {
        private StoresWrapperVerifier _verifier;
        public StoresWrapperVerifier Verifier => new StoresWrapperVerifier
        {
            CalledMethods = _verifier.CalledMethods
                |= _eventStore.Verifier.CalledMethods
                    |= _snapshotStore.Verifier.CalledMethods
        };

        private readonly ITransaction _transaction;
        private readonly EventStoreWrapper _eventStore;
        private readonly SnapshotStoreWrapper _snapshotStore;

        public IEventStore EventStore => _eventStore;
        public ISnapshotStore SnapshotStore => _snapshotStore;

        public StoresWrapper(
            ITransaction transaction, 
            IEventStore eventStore,
            ISnapshotStore snapshotStore)
        {
            _verifier = new StoresWrapperVerifier();

            _verifier.CalledMethods |= EventStoreMethods.Ctor;

            _transaction = transaction;

            _eventStore = new EventStoreWrapper(eventStore, _verifier);
            _snapshotStore = new SnapshotStoreWrapper(snapshotStore, _verifier);
            
        }

        public void Dispose()
        {
            EventStore.Dispose();

            _verifier.CalledMethods |= EventStoreMethods.Dispose;
        }

        public void BeginTransaction()
        {
            _transaction.BeginTransaction();

            _verifier.CalledMethods |= EventStoreMethods.BeginTransaction;
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();

            _verifier.CalledMethods |= EventStoreMethods.CommitAsync;
        }

        public void Rollback()
        {
            _transaction.Rollback();

            _verifier.CalledMethods |= EventStoreMethods.Rollback;
        }
        
        
    }

    public class EventStoreWrapper : IEventStore
    {
        private readonly IEventStore _store;

        public StoresWrapperVerifier Verifier;

        public EventStoreWrapper(IEventStore eventStore, StoresWrapperVerifier methods)
        {
            _store = eventStore;
            Verifier = methods;
        }
        
        public async Task<IEnumerable<ICommittedEvent>> GetAllEventsAsync(Guid id)
        {
            var result = await _store.GetAllEventsAsync(id).ConfigureAwait(false);

            Verifier.CalledMethods |= EventStoreMethods.GetAllEventsAsync;

            return result;
        }

        public Task<IEnumerable<ICommittedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            return Task.FromResult<IEnumerable<ICommittedEvent>>(null);
        }

        public async Task AppendAsync(IEnumerable<IUncommittedEvent> uncommittedEvents)
        {
            await _store.AppendAsync(uncommittedEvents).ConfigureAwait(false);

            Verifier.CalledMethods |= EventStoreMethods.SaveAsync;
        }

        public void Dispose()
        {
            _store.Dispose();

            Verifier.CalledMethods |= EventStoreMethods.Dispose;
        }
    }

    public class SnapshotStoreWrapper : ISnapshotStore
    {
        private readonly ISnapshotStore _store;

        public StoresWrapperVerifier Verifier;

        public SnapshotStoreWrapper(ISnapshotStore snapshotStore, StoresWrapperVerifier methods)
        {
            _store = snapshotStore;
            Verifier = methods;
        }
        
        public async Task SaveSnapshotAsync(IUncommittedSnapshot uncommittedSnapshot)
        {
            await _store.SaveSnapshotAsync(uncommittedSnapshot).ConfigureAwait(false);

            Verifier.CalledMethods |= EventStoreMethods.SaveSnapshotAsync;
        }

        public async Task<ICommittedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var result = await _store.GetLatestSnapshotByIdAsync(aggregateId).ConfigureAwait(false);

            Verifier.CalledMethods |= EventStoreMethods.GetLatestSnapshotByIdAsync;

            return result;
        }

        public async Task<IEnumerable<ICommittedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var result = await _store.GetEventsForwardAsync(aggregateId, version).ConfigureAwait(false);

            Verifier.CalledMethods |= EventStoreMethods.GetEventsForwardAsync;

            return result;
        }

        public void Dispose()
        {
            _store.Dispose();

            Verifier.CalledMethods |= EventStoreMethods.Dispose;
        }
    }
    
    public struct StoresWrapperVerifier
    {
        public EventStoreMethods CalledMethods { get; set; }
    }
}