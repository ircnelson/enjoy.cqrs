using System;
using System.Threading.Tasks;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Fixtures;
using EnjoyCQRS.IntegrationTests.Stubs.ApplicationLayer;
using EnjoyCQRS.IntegrationTests.Stubs.DomainLayer;
using EnjoyCQRS.MessageBus;
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
        public async Task Should_dispatch_command_and_retrieve_aggregate_from_repository()
        {
            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(Guid.NewGuid(), "Fake");

                DoDispatch(scope, command);
                
                var repository = scope.Resolve<IRepository>();
                
                var aggregateFromRepository = await repository.GetByIdAsync<FakePerson>(command.AggregateId).ConfigureAwait(false);

                aggregateFromRepository.Should().NotBeNull();

                aggregateFromRepository.Name.Should().Be(command.Name);
                aggregateFromRepository.Id.Should().Be(command.AggregateId);
            }
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public async Task Should_take_and_restore_snapshot()
        {
            var command = new CreateFakeGameCommand(Guid.NewGuid(), "Player 1", "Player 2");

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                DoDispatch(scope, command);

                _fixture.EventStore.SaveSnapshotCalled.Should().BeTrue();
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var repository = scope.Resolve<IRepository>();

                var aggregateFromRepository = await repository.GetByIdAsync<FakeGame>(command.AggregateId).ConfigureAwait(false);

                aggregateFromRepository.Should().NotBeNull();

                aggregateFromRepository.Id.Should().Be(command.AggregateId);
                aggregateFromRepository.NamePlayerOne.Should().Be(command.PlayerOneName);
                aggregateFromRepository.NamePlayerTwo.Should().Be(command.PlayerTwoName);

                _fixture.EventStore.GetSnapshotCalled.Should().BeTrue();
            }
        }

        private async void DoDispatch(ILifetimeScope scope, ICommand command)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandDispatcher = scope.Resolve<ICommandDispatcher>();

            await commandDispatcher.DispatchAsync(command).ConfigureAwait(false);
        }
    }
}