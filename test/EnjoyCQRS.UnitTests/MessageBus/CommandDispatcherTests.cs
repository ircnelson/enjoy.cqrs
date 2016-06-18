using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.MessageBus;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.MessageBus
{
    public class CommandDispatcherTests
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Command dispatcher";

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public async void When_a_single_Command_is_published_to_the_bus_containing_a_single_CommandHandler()
        {
            var handler = new FirstTestCommandHandler();

            List<Action<TestCommand>> handlers = new List<Action<TestCommand>>
            {
                command => handler.ExecuteAsync(command)
            };

            Mock<ICommandDispatcher> commandDispatcherMock = new Mock<ICommandDispatcher>();
            commandDispatcherMock.Setup(e => e.DispatchAsync(It.IsAny<TestCommand>())).Callback((ICommand command) =>
            {
                handlers.ForEach(action =>
                {
                    action((TestCommand) command);
                });
            }).Returns(Task.CompletedTask);

            var testCommand = new TestCommand(Guid.NewGuid());

            ICommandDispatcher commandDispatcher = commandDispatcherMock.Object;

            await commandDispatcher.DispatchAsync(testCommand);

            handler.Ids.First().Should().Be(testCommand.AggregateId);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public async void When_a_single_Command_is_published_to_the_bus_containing_multiple_CommandHandlers()
        {
            var handler1 = new FirstTestCommandHandler();
            var handler2 = new SecondTestCommandHandler();

            List<Action<TestCommand>> handlers = new List<Action<TestCommand>>
            {
                command => handler1.ExecuteAsync(command),
                command => handler2.ExecuteAsync(command)
            };

            Mock<ICommandDispatcher> commandDispatcherMock = new Mock<ICommandDispatcher>();
            commandDispatcherMock.Setup(e => e.DispatchAsync(It.IsAny<TestCommand>())).Callback((ICommand command) =>
            {
                handlers.ForEach(action =>
                {
                    action((TestCommand)command);
                });
            }).Returns(Task.CompletedTask);

            var testCommand = new TestCommand(Guid.NewGuid());

            ICommandDispatcher commandDispatcher = commandDispatcherMock.Object;

            await commandDispatcher.DispatchAsync(testCommand);

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