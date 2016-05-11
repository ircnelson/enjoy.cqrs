using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Bus.Direct;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Sqlite;
using EnjoyCQRS.IntegrationTests.Stubs;
using FluentAssertions;
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

            builder.RegisterGenericDecorator(
                typeof(TransactionalCommandHandler<>),
                typeof(ICommandHandler<>),
                fromKey: "uowCmdHandler");

            builder.RegisterAssemblyTypes(assemblyCommandHandlers)
                   .AsClosedTypesOf(typeof(ICommandHandler<>))
                   .AsSelf()
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope()
                   .Named("uowCmdHandler", typeof(ICommandHandler<>));

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

    public class CommandRouter : ICommandRouter
    {
        private readonly ILifetimeScope _scope;

        public CommandRouter(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public void Route(ICommand command)
        {
            var genericCommandHandler = typeof (ICommandHandler<>).MakeGenericType(command.GetType());

            var handler = _scope.ResolveOptional(genericCommandHandler) as ICommandHandler<ICommand>;
            
            handler.Execute(command);
        }
    }

    public class EventRouter : IEventRouter
    {
        private readonly ILifetimeScope _scope;

        public EventRouter(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public void Route(IDomainEvent @event)
        {
        }
    }

    public class TransactionalCommandHandler<TCommand> : ICommandHandler<TCommand> 
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _commandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionalCommandHandler(
            ICommandHandler<TCommand> commandHandler, 
            IUnitOfWork unitOfWork)
        {
            _commandHandler = commandHandler;
            _unitOfWork = unitOfWork;
        }

        public void Execute(TCommand command)
        {
            try
            {
                _commandHandler.Execute(command);

                _unitOfWork.Commit();
            }

            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }
        }
    }
}