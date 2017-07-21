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

using Autofac;
using System;
using EnjoyCQRS.Core;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.Logger;

namespace EnjoyCQRS.DependencyInjection.Autofac
{
    public class AutofacEnjoyDependenciesBuilder : EnjoyDependenciesBuilder<IComponentContext>
    {
        private readonly ContainerBuilder _builder;

        public AutofacEnjoyDependenciesBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventStore<TEventStore>()
        {
            return WithEventStore(typeof(TEventStore));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventStore<TEventStore>(Func<IComponentContext, TEventStore> instanceFactory)
        {
            _builder.Register(instanceFactory).As<IEventStore>().InstancePerLifetimeScope().IfNotRegistered(typeof(IEventStore));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventStore(Type type)
        {
            _builder.RegisterType(type).As<IEventStore>().InstancePerLifetimeScope().IfNotRegistered(typeof(IEventStore)); ;

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithCommandDispatcher<TCommandDispatcher>()
        {
            return WithCommandDispatcher(typeof(TCommandDispatcher));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithCommandDispatcher<TCommandDispatcher>(Func<IComponentContext, TCommandDispatcher> instanceFactory)
        {
            _builder.Register(instanceFactory).As<ICommandDispatcher>().InstancePerLifetimeScope().IfNotRegistered(typeof(ICommandDispatcher));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithCommandDispatcher(Type type)
        {
            _builder.RegisterType(type).As<ICommandDispatcher>().InstancePerLifetimeScope().IfNotRegistered(typeof(ICommandDispatcher));;

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithUnitOfWork<TUnitOfWork>()
        {
            return WithUnitOfWork(typeof(TUnitOfWork));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithUnitOfWork<TUnitOfWork>(Func<IComponentContext, TUnitOfWork> instanceFactory)
        {
            _builder.Register(instanceFactory).As<IUnitOfWork>().InstancePerLifetimeScope().IfNotRegistered(typeof(IUnitOfWork));;

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithUnitOfWork(Type type)
        {
            _builder.RegisterType(type).As<IUnitOfWork>().InstancePerLifetimeScope().IfNotRegistered(typeof(IUnitOfWork));;

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventRouter<TEventRouter>()
        {
            return WithEventRouter(typeof(TEventRouter));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventRouter<TEventRouter>(Func<IComponentContext, TEventRouter> instanceFactory)
        {
            _builder.Register(instanceFactory).As<IEventRouter>().InstancePerDependency().IfNotRegistered(typeof(IEventRouter));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventRouter(Type type)
        {
            _builder.RegisterType(type).As<IEventRouter>().InstancePerDependency().IfNotRegistered(typeof(IEventRouter));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventPublisher<TEventPublisher>()
        {
            return WithEventPublisher(typeof(TEventPublisher));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventPublisher<TEventPublisher>(Func<IComponentContext, TEventPublisher> instanceFactory)
        {
            _builder.Register(instanceFactory).As<IEventPublisher>().InstancePerLifetimeScope().IfNotRegistered(typeof(IEventPublisher));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventPublisher(Type type)
        {
            _builder.RegisterType(type).As<IEventPublisher>().InstancePerLifetimeScope().IfNotRegistered(typeof(IEventPublisher));

            return this;
        }
        
        public override IEnjoyDependenciesBuilder<IComponentContext> WithSnapshotStrategy<TSnapshotStrategy>()
        {
            return WithSnapshotStrategy(typeof(TSnapshotStrategy));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithSnapshotStrategy<TSnapshotStrategy>(Func<IComponentContext, TSnapshotStrategy> instanceFactory)
        {
            _builder.Register(instanceFactory).As<ISnapshotStrategy>().InstancePerDependency().IfNotRegistered(typeof(ISnapshotStrategy));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithSnapshotStrategy(Type type)
        {
            _builder.RegisterType(type).As<ISnapshotStrategy>().InstancePerDependency().IfNotRegistered(typeof(ISnapshotStrategy));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithSession<TSession>()
        {
            return WithSession(typeof(TSession));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithSession<TSession>(Func<IComponentContext, TSession> instanceFactory)
        {
            _builder.Register(instanceFactory).As<ISession>().InstancePerLifetimeScope().IfNotRegistered(typeof(ISession));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithSession(Type type)
        {
            _builder.RegisterType(type).As<ISession>().InstancePerLifetimeScope().IfNotRegistered(typeof(ISession));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithRepository<TRepository>()
        {
            return WithRepository(typeof(TRepository));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithRepository<TRepository>(Func<IComponentContext, TRepository> instanceFactory)
        {
            _builder.Register(instanceFactory).As<IRepository>().InstancePerDependency().IfNotRegistered(typeof(IRepository));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithRepository(Type type)
        {
            _builder.RegisterType(type).As<IRepository>().InstancePerDependency().IfNotRegistered(typeof(IRepository));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> AddMetadataProvider<TMetadataProvider>()
        {
            return AddMetadataProvider(typeof(TMetadataProvider));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> AddMetadataProvider<TMetadataProvider>(Func<IComponentContext, TMetadataProvider> instanceFactory)
        {
            _builder.Register(instanceFactory).As<IMetadataProvider>().InstancePerLifetimeScope();

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> AddMetadataProvider(Type type)
        {
            _builder.RegisterType(type).As<IMetadataProvider>().InstancePerLifetimeScope();

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventUpdateManager<TEventUpdateManager>()
        {
            return WithEventUpdateManager(typeof(TEventUpdateManager));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventUpdateManager<TEventUpdateManager>(Func<IComponentContext, TEventUpdateManager> instanceFactory)
        {
            _builder.Register(instanceFactory).As<IEventUpdateManager>().SingleInstance().IfNotRegistered(typeof(IEventUpdateManager));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventUpdateManager(Type type)
        {
            _builder.RegisterType(type).As<IEventUpdateManager>().SingleInstance().IfNotRegistered(typeof(IEventUpdateManager));;

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithLoggerFactory<TLoggerFactory>()
        {
            return WithLoggerFactory(typeof(TLoggerFactory));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithLoggerFactory<TLoggerFactory>(Func<IComponentContext, TLoggerFactory> instanceFactory)
        {
            _builder.Register(instanceFactory).As<ILoggerFactory>().InstancePerDependency().IfNotRegistered(typeof(ILoggerFactory));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithLoggerFactory(Type type)
        {
            _builder.RegisterType(type).As<ILoggerFactory>().InstancePerDependency().IfNotRegistered(typeof(ILoggerFactory));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventsMetadataService(Type type)
        {
            _builder.RegisterType(type).As<IEventsMetadataService>().InstancePerLifetimeScope().IfNotRegistered(typeof(IEventsMetadataService));

            return this;
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventsMetadataService<TImplementation>()
        {
            return WithEventsMetadataService(typeof(TImplementation));
        }

        public override IEnjoyDependenciesBuilder<IComponentContext> WithEventsMetadataService<TImplementation>(Func<IComponentContext, TImplementation> instanceFactory)
        {
            _builder.Register(instanceFactory).As<IEventsMetadataService>().InstancePerLifetimeScope().IfNotRegistered(typeof(IEventsMetadataService));

            return this;
        }
    }
}
