using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Bus.Direct;
using EnjoyCQRS.Commands;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.MessageBus
{
    public class CommandBusTests
    {
        [Fact]
        public void When_a_single_Command_is_published_to_the_bus_containing_a_single_CommandHandler()
        {
            var testCommand = new TestCommand(Guid.NewGuid());
            var handler = new FirstTestCommandHandler();
            DefaultRouterMessages routerMessages = new DefaultRouterMessages();
            routerMessages.Register<TestCommand>(x => handler.Execute(x));

            DirectMessageBus directMessageBus = new DirectMessageBus(routerMessages);

            directMessageBus.Dispatch(testCommand);
            directMessageBus.Commit();

            handler.Ids.First().Should().Be(testCommand.AggregateId);
        }

        [Fact]
        public void When_a_single_Command_is_published_to_the_bus_containing_multiple_CommandHandlers()
        {
            var testCommand = new TestCommand(Guid.NewGuid());
            var handler1 = new FirstTestCommandHandler();
            var handler2 = new SecondTestCommandHandler();

            DefaultRouterMessages routerMessages = new DefaultRouterMessages();
            routerMessages.Register<TestCommand>(x => handler1.Execute(x));
            routerMessages.Register<TestCommand>(x => handler2.Execute(x));

            DirectMessageBus directMessageBus = new DirectMessageBus(routerMessages);

            directMessageBus.Dispatch(testCommand);
            directMessageBus.Commit();

            handler1.Ids.First().Should().Be(testCommand.AggregateId);
            handler2.Ids.First().Should().Be(testCommand.AggregateId);
        }

        public class FirstTestCommandHandler : ICommandHandler<TestCommand>
        {
            public List<Guid> Ids { get; } = new List<Guid>();

            public void Execute(TestCommand command)
            {
                Ids.Add(command.AggregateId);
            }
        }

        public class SecondTestCommandHandler : ICommandHandler<TestCommand>
        {
            public List<Guid> Ids { get; } = new List<Guid>();

            public void Execute(TestCommand command)
            {
                Ids.Add(command.AggregateId);
            }
        }

        public class TestCommand : Command
        {
            public TestCommand(Guid id) : base(id)
            {
            }
        }
    }
}