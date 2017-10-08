using EnjoyCQRS.Core;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.MessageBus.InProcess;
using EnjoyCQRS.Stores;
using System;
using System.Collections.Generic;

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
            IEventUpdateManager eventUpdateManager = null,
            IEnumerable<IMetadataProvider> metadataProviders = null,
            ISnapshotStrategy snapshotStrategy = null,
            IEventsMetadataService eventsMetadataService = null)
        {
            return Create(transaction,
                stores.EventStore,
                stores.SnapshotStore,
                loggerFactory,
                eventPublisher,
                eventRouter,
                eventUpdateManager,
                metadataProviders,
                snapshotStrategy,
                eventsMetadataService);
        }

        public static ISession Create(
            ITransaction transaction,
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            ILoggerFactory loggerFactory = null,
            IEventPublisher eventPublisher = null,
            IEventRouter eventRouter = null,
            IEventUpdateManager eventUpdateManager = null,
            IEnumerable<IMetadataProvider> metadataProviders = null,
            ISnapshotStrategy snapshotStrategy = null,
            IEventsMetadataService eventsMetadataService = null)
        {
            // TODO: refactor ctor's Session sucks :(

            if (eventStore == null) throw new ArgumentNullException(nameof(eventStore));
            if (snapshotStore == null) throw new ArgumentNullException(nameof(snapshotStore));

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
                eventPublisher: eventPublisher,
                eventUpdateManager: eventUpdateManager,
                metadataProviders: metadataProviders,
                snapshotStrategy: snapshotStrategy,
                eventsMetadataService: eventsMetadataService);

            return session;
        }
    }
}
