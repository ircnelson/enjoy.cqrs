using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Bus.Direct;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
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
            var enumerabeGenericCommandHandler = typeof (IEnumerable<>).MakeGenericType(genericCommandHandler);

            var handlers = _scope.ResolveOptional(enumerabeGenericCommandHandler) as IEnumerable;


            foreach (var handler in handlers)
            {
                
                var methodInfo = handler.GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public);
                methodInfo.Invoke(handler, new[] {command});
            }
            
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

    public static class CustomRegistrationExtensions
    {
        // This is the important custom bit: Registering a named service during scanning.
        public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
            AsNamedClosedTypesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
                Type openGenericServiceType,
                Func<Type, object> keyFactory)
            where TScanningActivatorData : ScanningActivatorData
        {
            if (openGenericServiceType == null) throw new ArgumentNullException("openGenericServiceType");

            return registration
                .Where(candidateType => candidateType.IsClosedTypeOf(openGenericServiceType))
                .As(candidateType => candidateType.GetTypesThatClose(openGenericServiceType).Select(t => (Service)new KeyedService(keyFactory(t), t)));
        }

        // These next two methods are basically copy/paste of some Autofac internals that
        // are used to determine closed generic types during scanning.
        public static IEnumerable<Type> GetTypesThatClose(this Type candidateType, Type openGenericServiceType)
        {
            return candidateType.GetInterfaces().Concat(TraverseAcross(candidateType, t => t.BaseType)).Where(t => t.IsClosedTypeOf(openGenericServiceType));
        }

        public static IEnumerable<T> TraverseAcross<T>(T first, Func<T, T> next) where T : class
        {
            var item = first;
            while (item != null)
            {
                yield return item;
                item = next(item);
            }
        }
    }
}