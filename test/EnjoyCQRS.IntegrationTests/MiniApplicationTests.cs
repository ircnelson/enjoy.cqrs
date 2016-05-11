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
using Xunit;

namespace EnjoyCQRS.IntegrationTests
{
    public class MiniApplicationFixture : IDisposable
    {
        public ILifetimeScope Scope { get; private set; }

        public MiniApplicationFixture()
        {
            var fileName = "test.db";
            EventStoreSqliteInitializer eventStoreSqliteInitializer = new EventStoreSqliteInitializer(fileName);

            eventStoreSqliteInitializer.CreateDatabase(true);
            eventStoreSqliteInitializer.CreateTable();
            
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterType<Session>().As<ISession, IUnitOfWork>().InstancePerLifetimeScope();
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

            //builder.RegisterAssemblyTypes(assemblyCommandHandlers)
            //       .AsClosedTypesOf(genericCommandHandler)
            //       .AsSelf()
            //       .AsImplementedInterfaces()
            //       .InstancePerLifetimeScope()
            //       .Keyed("uowCmdHandler", genericCommandHandler);

            var container = builder.Build();

            Scope = container.BeginLifetimeScope();
        }
        
        public void Dispose()
        {
            Scope = null;
        }
    }

    public class MiniApplicationTests : IClassFixture<MiniApplicationFixture>
    {
        private readonly MiniApplicationFixture _fixture;

        public MiniApplicationTests(MiniApplicationFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Should_simulate_application()
        {
            var commandDispatcher = _fixture.Scope.Resolve<ICommandDispatcher>();

            var command = new CreateFakePersonCommand(Guid.NewGuid(), "Fake");

            commandDispatcher.Dispatch(command);
            commandDispatcher.Commit();
        }
    }
}