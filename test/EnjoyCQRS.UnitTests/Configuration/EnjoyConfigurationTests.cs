using System;
using System.Linq;
using Autofac;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Bus.Direct;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Configuration;
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
                typeof (CmdHandler1),
                typeof (CmdHandler2)
            });
            
            var handlerScanner = new HandlerScanner(enjoyTypeScannerMock.Object);
            var handlers = handlerScanner.Scan();

            var builder = new ContainerBuilder();

            var stubRegisterHandler = new StubRegisterHandler();

            builder.Register(c => stubRegisterHandler).As<IRegisterHandler>();
            builder.RegisterGeneric(typeof (TransactionHandler<,>));

            foreach (var handler in handlers.Values.SelectMany(e => e))
            {
                builder.RegisterType(handler).AsSelf().AsImplementedInterfaces();
            }

            var container = builder.Build();

            EnjoyConfiguration enjoyConfiguration = new EnjoyConfiguration(new AutofacResolver(container), handlers, enjoyTypeScannerMock.Object);

            enjoyConfiguration.Setup();

            stubRegisterHandler.Routes.Keys.Count().Should().Be(1);
            stubRegisterHandler.Routes[typeof (Cmd1)].Count.Should().Be(2);
        }
    }

    public class AutofacResolver : IResolver
    {
        private readonly IContainer _container;

        public AutofacResolver(IContainer container)
        {
            _container = container;
        }

        public TService Resolve<TService>()
        {
            return _container.Resolve<TService>();
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
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
