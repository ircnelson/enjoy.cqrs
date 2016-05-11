using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Bus.Direct;
using EnjoyCQRS.Commands;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.MessageBus
{
    public class CommandRouterTests
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Command router";

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void When_a_single_Command_is_published_to_the_bus_containing_a_single_CommandHandler()
        {
            var handler = new FirstTestCommandHandler();

            List<Action<TestCommand>> handlers = new List<Action<TestCommand>>
            {
                command => handler.Execute(command)
            };

            Mock<ICommandRouter> commandRouterMock = new Mock<ICommandRouter>();
            commandRouterMock.Setup(e => e.Route(It.IsAny<TestCommand>())).Callback((ICommand command) =>
            {
                handlers.ForEach((action =>
                {
                    action((TestCommand) command);
                }));
            });

            var testCommand = new TestCommand(Guid.NewGuid());
            DirectMessageBus directMessageBus = new DirectMessageBus(commandRouterMock.Object, It.IsAny<IEventRouter>());

            directMessageBus.Dispatch(testCommand);
            directMessageBus.Commit();

            handler.Ids.First().Should().Be(testCommand.AggregateId);
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void When_a_single_Command_is_published_to_the_bus_containing_multiple_CommandHandlers()
        {
            var handler1 = new FirstTestCommandHandler();
            var handler2 = new SecondTestCommandHandler();

            List<Action<TestCommand>> handlers = new List<Action<TestCommand>>
            {
                command => handler1.Execute(command),
                command => handler2.Execute(command)
            };

            Mock<ICommandRouter> commandRouterMock = new Mock<ICommandRouter>();
            commandRouterMock.Setup(e => e.Route(It.IsAny<TestCommand>())).Callback((ICommand command) =>
            {
                handlers.ForEach((action =>
                {
                    action((TestCommand)command);
                }));
            });

            var testCommand = new TestCommand(Guid.NewGuid());
            
            DirectMessageBus directMessageBus = new DirectMessageBus(commandRouterMock.Object, It.IsAny<IEventRouter>());

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