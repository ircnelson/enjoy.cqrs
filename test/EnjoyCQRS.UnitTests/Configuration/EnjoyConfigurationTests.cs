using System;
using System.Linq;
using Autofac;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Configuration;
using EnjoyCQRS.EventSource.Storage;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Configuration
{
    public class EnjoyConfigurationTests
    {
        [Fact]
        public void Should_scan_all_handlers()
        {
            var enjoyTypeScannerMock = new Mock<IEnjoyTypeScanner>();
            enjoyTypeScannerMock.Setup(e => e.Scan()).Returns(() => new[]
            {
                typeof (Cmd1),
                typeof (CmdHandler1),
                typeof (CmdHandler2)
            });


            var handlerScanner = new HandlerScanner(enjoyTypeScannerMock.Object);
            var handlers = handlerScanner.Scan();

            handlers.Count().Should().Be(1);
            handlers[new HandlerMetadata(typeof(Cmd1), HandlerType.Command)].Count().Should().Be(2);
        }

        [Fact]
        public void Should_wrap_handlers()
        {
            var enjoyTypeScannerMock = new Mock<IEnjoyTypeScanner>();
            enjoyTypeScannerMock.Setup(e => e.Scan()).Returns(() => new[]
            {
                typeof (Cmd1),
                typeof (CmdHandler1)
            });

            var transactionalCommandHandlerFactoryMock = new Mock<IDecorateCommandHandler>();
            
            var handlerScanner = new HandlerScanner(enjoyTypeScannerMock.Object);
            var handlers = handlerScanner.Scan();

            var builder = new ContainerBuilder();

            var stubRegisterHandler = new StubRegisterHandler();

            builder.Register(c => transactionalCommandHandlerFactoryMock.Object).As<IDecorateCommandHandler>();
            builder.Register(c => stubRegisterHandler).As<IRegisterHandler>();

            foreach (var handler in handlers.Values.SelectMany(e => e))
            {
                builder.RegisterType(handler).AsSelf().AsImplementedInterfaces();
            }

            var container = builder.Build();

            EnjoyConfiguration enjoyConfiguration = new EnjoyConfiguration(new AutofacScopeResolver(container), handlers, enjoyTypeScannerMock.Object);

            enjoyConfiguration.Setup();
            
            transactionalCommandHandlerFactoryMock.Verify(e => e.Decorate<Cmd1, CmdHandler1>(It.IsAny<CmdHandler1>()));
        }
    }

    public class AutofacScopeResolver : AutofacResolver, IScopeResolver
    {
        private readonly ILifetimeScope _lifetimeScope;

        public AutofacScopeResolver(ILifetimeScope lifetimeScope) : base(lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void Dispose()
        {
            _lifetimeScope.Dispose();
        }

        public IScopeResolver BeginScope()
        {
            return new AutofacScopeResolver(_lifetimeScope.BeginLifetimeScope());
        }
    }

    public class AutofacResolver : IResolver
    {
        private readonly IComponentContext _componentContext;

        public AutofacResolver(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public TService Resolve<TService>()
        {
            return _componentContext.Resolve<TService>();
        }

        public object Resolve(Type type)
        {
            return _componentContext.Resolve(type);
        }
    }

    public class Cmd1 : Command
    {
        public Cmd1(Guid aggregateId) : base(aggregateId)
        {
        }
    }

    public class CmdHandler1 : ICommandHandler<Cmd1>
    {
        public void Execute(Cmd1 command)
        {
        }
    }

    public class CmdHandler2 : ICommandHandler<Cmd1>
    {
        public void Execute(Cmd1 command)
        {
        }
    }
}
