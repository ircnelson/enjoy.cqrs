using System;
using System.Collections.Generic;
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
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.IntegrationTests
{
    public class MiniApplicationFixture : IDisposable
    {
        public IContainer Container { get; private set; }

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

            Container = container;
        }
        
        public void Dispose()
        {
            Container.Dispose();
            Container = null;
        }
    }

    public class MiniApplicationTests : IClassFixture<MiniApplicationFixture>
    {
        private const string CategoryName = "Integration";
        private const string CategoryValue = "Simulation";

        private readonly MiniApplicationFixture _fixture;

        public MiniApplicationTests(MiniApplicationFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Should_retrieves_the_aggregate_from_tracking()
        {
            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(Guid.NewGuid(), "Fake");

                DoDispatch(scope, command);
                
                var repository = scope.Resolve<IRepository>();
                var aggregateTracker = scope.Resolve<IAggregateTracker>();

                var aggregateFromTracking = aggregateTracker.GetById<FakePerson>(command.AggregateId);
                var aggregateFromRepository = repository.GetById<FakePerson>(command.AggregateId);
                
                aggregateFromTracking.Should().BeSameAs(aggregateFromRepository);
            }
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Aggregate_cannot_exists_in_tracking_on_another_scope()
        {
            var aggregateId = Guid.NewGuid();

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(aggregateId, "Fake");

                DoDispatch(scope, command);
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var aggregateTracker = scope.Resolve<IAggregateTracker>();
                var aggregateFromTracking = aggregateTracker.GetById<FakePerson>(aggregateId);

                aggregateFromTracking.Should().BeNull();
            }
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Should_retrieves_an_aggregate_from_EventStore_When_it_was_not_tracked()
        {
            var aggregateId = Guid.NewGuid();

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(aggregateId, "Fake");

                DoDispatch(scope, command);
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var aggregateTracker = scope.Resolve<IAggregateTracker>();
                var aggregateFromTracking = aggregateTracker.GetById<FakePerson>(aggregateId);

                aggregateFromTracking.Should().BeNull();
            }
        }

        private void DoDispatch(ILifetimeScope scope, ICommand command)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandDispatcher = scope.Resolve<ICommandDispatcher>();

            commandDispatcher.Dispatch(command);
            commandDispatcher.Commit();
        }
    }
}