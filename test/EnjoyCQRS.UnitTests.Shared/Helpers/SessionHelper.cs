using EnjoyCQRS.Core;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.MessageBus.InProcess;
using System;
using System.Collections.Generic;

namespace EnjoyCQRS.UnitTests.Shared.Helpers
{
    public class SessionHelper
    {
        public static ISession Create(IEventStore eventStore, 
            ILoggerFactory loggerFactory = null,
            IEventPublisher eventPublisher = null,
            IEventRouter eventRouter = null,
            IEventSerializer eventSerializer = null,
            ISnapshotSerializer snapshotSerializer = null,
            IProjectionSerializer projectionSerializer = null,
            IProjectionProviderScanner projectionProviderScanner = null,
            IEventUpdateManager eventUpdateManager = null,
            IEnumerable<IMetadataProvider> metadataProviders = null,
            ISnapshotStrategy snapshotStrategy = null,
            IEventsMetadataService eventsMetadataService = null)
        {
            // TODO: refactor ctor's Session sucks :(

            if (eventStore == null) throw new ArgumentNullException(nameof(eventStore));

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

            if (eventSerializer == null)
            {
                eventSerializer = new EventSerializer(new JsonTextSerializer());
            }

            if (snapshotSerializer == null)
            {
                snapshotSerializer = new SnapshotSerializer(new JsonTextSerializer());
            }

            if (projectionSerializer == null)
            {
                projectionSerializer = new ProjectionSerializer(new JsonTextSerializer());
            }

            var session = new Session(loggerFactory: loggerFactory,
                eventStore: eventStore,
                eventPublisher: eventPublisher,
                eventSerializer: eventSerializer,
                snapshotSerializer: snapshotSerializer,
                projectionSerializer: projectionSerializer,
                projectionProviderScanner: projectionProviderScanner,
                eventUpdateManager: eventUpdateManager,
                metadataProviders: metadataProviders,
                snapshotStrategy: snapshotStrategy,
                eventsMetadataService: eventsMetadataService);

            return session;
        }
    }
}
