using System;
using Autofac;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Bus.Direct;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Extensions;
using EnjoyCQRS.IntegrationTests.Sqlite;
using EnjoyCQRS.IntegrationTests.Stubs;

namespace EnjoyCQRS.IntegrationTests.Fixtures
{
    public class MiniApplicationFixture : IDisposable
    {
        public IContainer Container { get; private set; }

        public MiniApplicationFixture()
        {
            const string fileName = "test.db";

            var eventStoreSqliteInitializer = new EventStoreSqliteInitializer(fileName);

            eventStoreSqliteInitializer.CreateDatabase(true);
            eventStoreSqliteInitializer.CreateTable();
            
            var builder = new ContainerBuilder();

            builder.RegisterType<StubUnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder.RegisterType<Session>().As<ISession>().InstancePerLifetimeScope();
            builder.RegisterType<AggregateTracker>().As<IAggregateTracker>().InstancePerLifetimeScope();
            builder.RegisterType<Repository>().As<IRepository>();
            builder.RegisterType<DirectMessageBus>().As<ICommandDispatcher, IEventPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<CommandRouter>().As<ICommandRouter>();
            builder.RegisterType<EventRouter>().As<IEventRouter>();
            builder.Register(c => new EventStoreSqlite(fileName)).As<IEventStore>();

            var assemblyCommandHandlers = typeof (CreateFakePersonCommandHandler).Assembly;

            var genericCommandHandler = typeof (ICommandHandler<>);

            builder.RegisterAssemblyTypes(assemblyCommandHandlers)
                .AsNamedClosedTypesOf(genericCommandHandler, t => "uowCmdHandler");
            
            builder.RegisterGenericDecorator(
                typeof(TransactionalCommandHandler<>),
                genericCommandHandler,
                fromKey: "uowCmdHandler");

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