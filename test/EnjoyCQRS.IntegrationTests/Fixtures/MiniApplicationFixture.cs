using System;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Extensions;
using EnjoyCQRS.IntegrationTests.Infrastructure;
using EnjoyCQRS.IntegrationTests.Shared;
using EnjoyCQRS.IntegrationTests.Sqlite;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.MessageBus.InProcess;

namespace EnjoyCQRS.IntegrationTests.Fixtures
{
    public class MiniApplicationFixture : IDisposable
    {
        public IContainer Container { get; private set; }
        public EventStoreSqlite EventStore { get; set; }

        public IntervalSnapshotStrategy SnapshotStrategy { get; set; } = new IntervalSnapshotStrategy();

        public MiniApplicationFixture()
        {
            const string fileName = "test.db";

            var eventStoreSqliteInitializer = new EventStoreSqliteInitializer(fileName);

            eventStoreSqliteInitializer.CreateDatabase(true);
            eventStoreSqliteInitializer.CreateTables();
            
            var builder = new ContainerBuilder();
            
            builder.Register(c => SnapshotStrategy).As<ISnapshotStrategy>();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder.RegisterType<Session>().As<ISession>().InstancePerLifetimeScope();
            builder.RegisterType<Repository>().As<IRepository>();
            builder.RegisterType<StubCommandDispatcher>().As<ICommandDispatcher>().InstancePerLifetimeScope();
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<EventRouter>().As<IEventRouter>();
            builder.RegisterType<EventSerializer>().As<IEventSerializer>();
            builder.RegisterType<JsonTextSerializer>().As<ITextSerializer>();
            builder.RegisterType<NoopLoggerFactory>().As<ILoggerFactory>().InstancePerLifetimeScope();

            builder.Register(c => new EventStoreSqlite(fileName)).As<IEventStore>().OnActivated(args =>
            {
                EventStore = args.Instance;
            });

            var assemblyCommandHandlers = typeof (FooAssembler).Assembly;

            // Command handlers
            var genericCommandHandler = typeof (ICommandHandler<>);
            
            builder.RegisterAssemblyTypes(assemblyCommandHandlers)
                .AsNamedClosedTypesOf(genericCommandHandler, t => "uowCmdHandler");
            
            builder.RegisterGenericDecorator(
                typeof(TransactionalCommandHandler<>),
                genericCommandHandler,
                fromKey: "uowCmdHandler");

            // Event handlers
            var genericEventHandler = typeof(IEventHandler<>);

            builder.RegisterAssemblyTypes(assemblyCommandHandlers)
                   .AsClosedTypesOf(genericEventHandler)
                   .AsImplementedInterfaces();
            
            var container = builder.Build();

            Container = container;
        }
        
        public void Dispose()
        {
            Container.Dispose();
            Container = null;
        }
    }
}