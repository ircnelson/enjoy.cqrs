using EnjoyCQRS.Core;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.MessageBus.InProcess;
using EnjoyCQRS.Stores;
using System;
using System.Collections.Generic;
using IProjectionStoreV1 = EnjoyCQRS.EventSource.Projections.IProjectionStore;

namespace EnjoyCQRS.UnitTests.Shared.Helpers
{
    public class SessionHelper
    {
        public static ISession Create(
            ITransaction transaction,
            ICompositeStores stores,
            ILoggerFactory loggerFactory = null,
            IEventPublisher eventPublisher = null,
            IEventRouter eventRouter = null,
            IProjectionProviderScanner projectionProviderScanner = null,
            IEventUpdateManager eventUpdateManager = null,
            IEnumerable<IMetadataProvider> metadataProviders = null,
            ISnapshotStrategy snapshotStrategy = null,
            IEventsMetadataService eventsMetadataService = null)
        {
            return Create(transaction,
                stores.EventStore,
                stores.SnapshotStore,
                stores.ProjectionStoreV1,
                loggerFactory,
                eventPublisher,
                eventRouter,
                projectionProviderScanner,
                eventUpdateManager,
                metadataProviders,
                snapshotStrategy,
                eventsMetadataService);
        }

        public static ISession Create(
            ITransaction transaction,
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            IProjectionStoreV1 projectionStoreV1,
            ILoggerFactory loggerFactory = null,
            IEventPublisher eventPublisher = null,
            IEventRouter eventRouter = null,
            IProjectionProviderScanner projectionProviderScanner = null,
            IEventUpdateManager eventUpdateManager = null,
            IEnumerable<IMetadataProvider> metadataProviders = null,
            ISnapshotStrategy snapshotStrategy = null,
            IEventsMetadataService eventsMetadataService = null)
        {
            // TODO: refactor ctor's Session sucks :(

            if (eventStore == null) throw new ArgumentNullException(nameof(eventStore));
            if (snapshotStore == null) throw new ArgumentNullException(nameof(snapshotStore));
            if (projectionStoreV1 == null) throw new ArgumentNullException(nameof(projectionStoreV1));

            if (loggerFactory == null)
            {
                loggerFactory = new NoopLoggerFactory();
            }

            if (eventRouter == null)
            {
                eventRouter = StubEventRouter.Ok();
            }

            if (eventPublisher == null)
            {
                eventPublisher = new EventPublisher(eventRouter);
            }
            
            var session = new Session(
                loggerFactory: loggerFactory,
                transaction: transaction,
                eventStore: eventStore,
                snapshotStore: snapshotStore,
                projectionStoreV1: projectionStoreV1,
                eventPublisher: eventPublisher,
                projectionProviderScanner: projectionProviderScanner,
                eventUpdateManager: eventUpdateManager,
                metadataProviders: metadataProviders,
                snapshotStrategy: snapshotStrategy,
                eventsMetadataService: eventsMetadataService);

            return session;
        }
    }
}
