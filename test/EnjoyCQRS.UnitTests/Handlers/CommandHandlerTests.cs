using System;
using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Handlers
{
    public class CommandHandlerTests : CommandTestFixture<CommandHandlerTests.CreateStubCommand, CommandHandlerTests.CreateStubCommandHandler, StubAggregate>
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Handlers";

        private Guid _id;

        protected override CreateStubCommand When()
        {
            _id = Guid.NewGuid();

            return new CreateStubCommand(_id);
        }
        
        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Executed_property_should_be_true()
        {
            CommandHandler.Executed.Should().Be(true);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_pass_the_correct_AggregateId()
        {
            CommandHandler.AggregateId.Should().Be(_id);
        }

        public class CreateStubCommand : Command
        {
            public CreateStubCommand(Guid aggregateId) : base(aggregateId)
            {
            }
        }

        public class CreateStubCommandHandler : ICommandHandler<CreateStubCommand>
        {
            public bool Executed { get; set; }
            public Guid AggregateId { get; set; }

            public Task ExecuteAsync(CreateStubCommand command)
            {
                Executed = true;
                AggregateId = command.AggregateId;

                return Task.CompletedTask;
            }
        }
    }
}