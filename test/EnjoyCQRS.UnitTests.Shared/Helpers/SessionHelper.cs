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
            
            var session = new Session(loggerFactory: loggerFactory,
                eventStore: eventStore,
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
