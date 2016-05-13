using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Bus.InProcess;
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
                command => handler.ExecuteAsync(command)
            };

            Mock<ICommandRouter> commandRouterMock = new Mock<ICommandRouter>();
            commandRouterMock.Setup(e => e.RouteAsync(It.IsAny<TestCommand>())).Callback((ICommand command) =>
            {
                handlers.ForEach((action =>
                {
                    action((TestCommand) command);
                }));
            });

            var testCommand = new TestCommand(Guid.NewGuid());
            CommandBus directMessageBus = new CommandBus(commandRouterMock.Object);

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
                command => handler1.ExecuteAsync(command),
                command => handler2.ExecuteAsync(command)
            };

            Mock<ICommandRouter> commandRouterMock = new Mock<ICommandRouter>();
            commandRouterMock.Setup(e => e.RouteAsync(It.IsAny<TestCommand>())).Callback((ICommand command) =>
            {
                handlers.ForEach((action =>
                {
                    action((TestCommand)command);
                }));
            });

            var testCommand = new TestCommand(Guid.NewGuid());

            CommandBus directMessageBus = new CommandBus(commandRouterMock.Object);

            directMessageBus.Dispatch(testCommand);
            directMessageBus.Commit();

            handler1.Ids.First().Should().Be(testCommand.AggregateId);
            handler2.Ids.First().Should().Be(testCommand.AggregateId);
        }

        public class FirstTestCommandHandler : ICommandHandler<TestCommand>
        {
            public List<Guid> Ids { get; } = new List<Guid>();

            public Task ExecuteAsync(TestCommand command)
            {
                Ids.Add(command.AggregateId);

                return Task.CompletedTask;
            }
        }

        public class SecondTestCommandHandler : ICommandHandler<TestCommand>
        {
            public List<Guid> Ids { get; } = new List<Guid>();

            public Task ExecuteAsync(TestCommand command)
            {
                Ids.Add(command.AggregateId);

                return Task.CompletedTask;
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