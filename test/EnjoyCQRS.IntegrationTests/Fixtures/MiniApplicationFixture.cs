using System;
using Autofac;
using Autofac.Features.Variance;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Bus.InProcess;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Extensions;
using EnjoyCQRS.IntegrationTests.Sqlite;
using EnjoyCQRS.IntegrationTests.Stubs;
using EnjoyCQRS.Messages;

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
            builder.RegisterType<Repository>().As<IRepository>();
            builder.RegisterType<StubCommandBus>().As<ICommandDispatcher>().InstancePerLifetimeScope();
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<EventRouter>().As<IEventRouter>();
            builder.Register(c => new EventStoreSqlite(fileName)).As<IEventStore>();

            var assemblyCommandHandlers = typeof (FakePerson).Assembly;

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