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
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using System.Linq;

namespace EnjoyCQRS.Core
{
    public abstract class EnjoyDependenciesBuilder<TResolver> : IEnjoyDependenciesBuilder<TResolver>
    {
        protected readonly EnjoyDependencies Dependencies = new EnjoyDependencies();

        public EnjoyDependencies Build()
        {
            if (Dependencies.NotRegistered.Any())
            {
                var exceptions = Dependencies.NotRegistered.Select(e => new Exception($"EnjoyCQRS dependency '{e.DependencyType}' not registered."));

                throw new AggregateException(exceptions);
            }

            return Dependencies;
        }

        #region EventStore

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventStore(Type type)
        {
            Dependencies.EventStore.MarkAsRegistered();

            return WithEventStore(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventStore<TEventStore>()
        {
            Dependencies.EventStore.MarkAsRegistered();

            return WithEventStore<TEventStore>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventStore<TEventStore>(Func<TResolver, TEventStore> instanceFactory)
        {
            Dependencies.EventStore.MarkAsRegistered();

            return WithEventStore(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventStore(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventStore<TImplementation>() where TImplementation : class, IEventStore;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventStore<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventStore;

        #endregion EventStore

        #region CommandDispatcher

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithCommandDispatcher(Type type)
        {
            Dependencies.CommandDispatcher.MarkAsRegistered();

            return WithCommandDispatcher(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithCommandDispatcher<TCommandDispatcher>()
        {
            Dependencies.CommandDispatcher.MarkAsRegistered();

            return WithCommandDispatcher<TCommandDispatcher>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithCommandDispatcher<TCommandDispatcher>(Func<TResolver, TCommandDispatcher> instanceFactory)
        {
            Dependencies.CommandDispatcher.MarkAsRegistered();

            return WithCommandDispatcher(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithCommandDispatcher(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithCommandDispatcher<TImplementation>() where TImplementation : class, ICommandDispatcher;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithCommandDispatcher<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, ICommandDispatcher;

        #endregion CommandDispatcher

        #region UnitOfWork

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithUnitOfWork(Type type)
        {
            Dependencies.UnitOfWork.MarkAsRegistered();

            return WithUnitOfWork(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithUnitOfWork<TUnitOfWork>()
        {
            Dependencies.UnitOfWork.MarkAsRegistered();

            return WithUnitOfWork<TUnitOfWork>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithUnitOfWork<TUnitOfWork>(Func<TResolver, TUnitOfWork> instanceFactory)
        {
            Dependencies.UnitOfWork.MarkAsRegistered();

            return WithUnitOfWork(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithUnitOfWork(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithUnitOfWork<TImplementation>() where TImplementation : class, IUnitOfWork;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithUnitOfWork<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IUnitOfWork;

        #endregion UnitOfWork

        #region EventRouter

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventRouter(Type type)
        {
            Dependencies.EventRouter.MarkAsRegistered();

            return WithEventRouter(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventRouter<TEventRouter>()
        {
            Dependencies.EventRouter.MarkAsRegistered();

            return WithEventRouter<TEventRouter>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventRouter<TEventRouter>(Func<TResolver, TEventRouter> instanceFactory)
        {
            Dependencies.EventRouter.MarkAsRegistered();

            return WithEventRouter(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventRouter(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventRouter<TImplementation>() where TImplementation : class, IEventRouter;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventRouter<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventRouter;

        #endregion EventRouter

        #region EventPublisher

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventPublisher(Type type)
        {
            Dependencies.EventPublisher.MarkAsRegistered();

            return WithEventPublisher(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventPublisher<TEventPublisher>()
        {
            Dependencies.EventPublisher.MarkAsRegistered();

            return WithEventPublisher<TEventPublisher>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventPublisher<TEventPublisher>(Func<TResolver, TEventPublisher> instanceFactory)
        {
            Dependencies.EventPublisher.MarkAsRegistered();

            return WithEventPublisher(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventPublisher(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventPublisher<TImplementation>() where TImplementation : class, IEventPublisher;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventPublisher<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventPublisher;

        #endregion EventPublisher
        
        #region SnapshotStrategy

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithSnapshotStrategy(Type type)
        {
            Dependencies.SnapshotStrategy.MarkAsRegistered();

            return WithSnapshotStrategy(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithSnapshotStrategy<TSnapshotStrategy>()
        {
            Dependencies.SnapshotStrategy.MarkAsRegistered();

            return WithSnapshotStrategy<TSnapshotStrategy>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithSnapshotStrategy<TSnapshotStrategy>(Func<TResolver, TSnapshotStrategy> instanceFactory)
        {
            Dependencies.SnapshotStrategy.MarkAsRegistered();

            return WithSnapshotStrategy(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithSnapshotStrategy(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithSnapshotStrategy<TImplementation>() where TImplementation : class, ISnapshotStrategy;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithSnapshotStrategy<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, ISnapshotStrategy;

        #endregion SnapshotStrategy

        #region Session

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithSession(Type type)
        {
            Dependencies.Session.MarkAsRegistered();

            return WithSession(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithSession<TSession>()
        {
            Dependencies.Session.MarkAsRegistered();

            return WithSession<TSession>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithSession<TSession>(Func<TResolver, TSession> instanceFactory)
        {
            Dependencies.Session.MarkAsRegistered();

            return WithSession(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithSession(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithSession<TImplementation>() where TImplementation : class, ISession;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithSession<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, ISession;

        #endregion Session

        #region Repository

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithRepository(Type type)
        {
            Dependencies.Repository.MarkAsRegistered();

            return WithRepository(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithRepository<TRepository>()
        {
            Dependencies.Repository.MarkAsRegistered();

            return WithRepository<TRepository>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithRepository<TRepository>(Func<TResolver, TRepository> instanceFactory)
        {
            Dependencies.Repository.MarkAsRegistered();

            return WithRepository(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithRepository(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithRepository<TImplementation>() where TImplementation : class, IRepository;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithRepository<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IRepository;

        #endregion Repository

        #region MetadataProvider

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.AddMetadataProvider(Type type)
        {
            return AddMetadataProvider(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.AddMetadataProvider<TMetadataProvider>()
        {
            return AddMetadataProvider<TMetadataProvider>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.AddMetadataProvider<TMetadataProvider>(Func<TResolver, TMetadataProvider> instanceFactory)
        {
            return AddMetadataProvider(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> AddMetadataProvider(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> AddMetadataProvider<TImplementation>() where TImplementation : class, IMetadataProvider;
        public abstract IEnjoyDependenciesBuilder<TResolver> AddMetadataProvider<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IMetadataProvider;

        #endregion MetadataProvider

        #region EventUpdateManager

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventUpdateManager(Type type)
        {
            Dependencies.EventUpdateManager.MarkAsRegistered();

            return WithEventUpdateManager(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventUpdateManager<TEventUpdateManager>()
        {
            Dependencies.EventUpdateManager.MarkAsRegistered();

            return WithEventUpdateManager<TEventUpdateManager>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventUpdateManager<TEventUpdateManager>(Func<TResolver, TEventUpdateManager> instanceFactory)
        {
            Dependencies.EventUpdateManager.MarkAsRegistered();

            return WithEventUpdateManager(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventUpdateManager(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventUpdateManager<TImplementation>() where TImplementation : class, IEventUpdateManager;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventUpdateManager<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventUpdateManager;

        #endregion EventUpdateManager

        #region EventsMetadataService

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventsMetadataService(Type type)
        {
            Dependencies.EventsMetadataService.MarkAsRegistered();

            return WithEventsMetadataService(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventsMetadataService<TEventsMetadataService>()
        {
            Dependencies.EventsMetadataService.MarkAsRegistered();

            return WithEventsMetadataService<TEventsMetadataService>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithEventsMetadataService<TEventUpdateManager>(Func<TResolver, TEventUpdateManager> instanceFactory)
        {
            Dependencies.EventsMetadataService.MarkAsRegistered();

            return WithEventsMetadataService(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventsMetadataService(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventsMetadataService<TImplementation>() where TImplementation : class, IEventsMetadataService;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithEventsMetadataService<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventsMetadataService;

        #endregion EventsMetadataService

        #region LoggerFactory

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithLoggerFactory(Type type)
        {
            Dependencies.LoggerFactory.MarkAsRegistered();

            return WithLoggerFactory(type);
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithLoggerFactory<TLoggerFactory>()
        {
            Dependencies.LoggerFactory.MarkAsRegistered();

            return WithLoggerFactory<TLoggerFactory>();
        }

        IEnjoyDependenciesBuilder<TResolver> IEnjoyDependenciesBuilder<TResolver>.WithLoggerFactory<TLoggerFactory>(Func<TResolver, TLoggerFactory> instanceFactory)
        {
            Dependencies.LoggerFactory.MarkAsRegistered();

            return WithLoggerFactory(instanceFactory);
        }

        public abstract IEnjoyDependenciesBuilder<TResolver> WithLoggerFactory(Type type);
        public abstract IEnjoyDependenciesBuilder<TResolver> WithLoggerFactory<TImplementation>() where TImplementation : class, ILoggerFactory;
        public abstract IEnjoyDependenciesBuilder<TResolver> WithLoggerFactory<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, ILoggerFactory;

        #endregion LoggerFactory
    }
}
