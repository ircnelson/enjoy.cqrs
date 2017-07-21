using EnjoyCQRS.Core;
using System;

namespace EnjoyCQRS.UnitTests.Core.Stubs
{
    class StubEnjoyDependenciesBuilder : EnjoyDependenciesBuilder<IServiceProvider>
    {
        public override IEnjoyDependenciesBuilder<IServiceProvider> AddMetadataProvider(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> AddMetadataProvider<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> AddMetadataProvider<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithCommandDispatcher(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithCommandDispatcher<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithCommandDispatcher<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventPublisher(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventPublisher<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventPublisher<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventRouter(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventRouter<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventRouter<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }
        
        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventsMetadataService(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventsMetadataService<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventsMetadataService<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventStore(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventStore<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventStore<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventUpdateManager(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventUpdateManager<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithEventUpdateManager<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithLoggerFactory(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithLoggerFactory<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithLoggerFactory<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }
        
        public override IEnjoyDependenciesBuilder<IServiceProvider> WithRepository(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithRepository<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithRepository<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithSession(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithSession<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithSession<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }
        
        public override IEnjoyDependenciesBuilder<IServiceProvider> WithSnapshotStrategy(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithSnapshotStrategy<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithSnapshotStrategy<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }
        
        public override IEnjoyDependenciesBuilder<IServiceProvider> WithUnitOfWork(Type type)
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithUnitOfWork<TImplementation>()
        {
            return this;
        }

        public override IEnjoyDependenciesBuilder<IServiceProvider> WithUnitOfWork<TImplementation>(Func<IServiceProvider, TImplementation> instanceFactory)
        {
            return this;
        }
    }
}
