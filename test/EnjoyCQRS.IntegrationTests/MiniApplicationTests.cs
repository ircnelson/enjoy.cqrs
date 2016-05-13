using System;
using System.Threading.Tasks;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Fixtures;
using EnjoyCQRS.IntegrationTests.Stubs;
using EnjoyCQRS.Messages;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.IntegrationTests
{
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
        public async Task Should_simulate_application()
        {
            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(Guid.NewGuid(), "Fake");

                DoDispatch(scope, command);
                
                var repository = scope.Resolve<IRepository>();
                
                var aggregateFromRepository = await repository.GetByIdAsync<FakePerson>(command.AggregateId);

                aggregateFromRepository.Should().NotBeNull();

                aggregateFromRepository.Name.Should().Be(command.Name);
                aggregateFromRepository.Id.Should().Be(command.AggregateId);
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