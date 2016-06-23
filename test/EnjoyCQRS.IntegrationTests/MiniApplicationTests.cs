using System;
using System.Threading.Tasks;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Fixtures;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FakeGameAggregate;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FooAggregate;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FakeGameAggregate;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FooAggregate;
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
                var command = new CreateFooCommand(Guid.NewGuid());

                DoDispatch(scope, command);
                
                var repository = scope.Resolve<IRepository>();
                
                var aggregateFromRepository = await repository.GetByIdAsync<Foo>(command.AggregateId).ConfigureAwait(false);

                aggregateFromRepository.Should().NotBeNull();
                
                aggregateFromRepository.Id.Should().Be(command.AggregateId);
            }
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public async Task Should_take_and_restore_snapshot_based_on_interval_strategy_configured()
        {
            _fixture.SnapshotStrategy = new IntervalSnapshotStrategy(5);

            var aggregateId = Guid.NewGuid();

            var command = new CreateFakeGameCommand(aggregateId, "Player 1", "Player 2");

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                DoDispatch(scope, command);

                _fixture.EventStore.SaveSnapshotCalled.Should().BeFalse();
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                DoDispatch(scope, new FloodChangePlayerName(aggregateId, 1, "Player", _fixture.SnapshotStrategy.SnapshotInterval));
                
                _fixture.EventStore.SaveSnapshotCalled.Should().BeTrue();
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var repository = scope.Resolve<IRepository>();

                var aggregateFromRepository = await repository.GetByIdAsync<FakeGame>(aggregateId).ConfigureAwait(false);

                aggregateFromRepository.Should().NotBeNull();

                aggregateFromRepository.Id.Should().Be(command.AggregateId);
                aggregateFromRepository.NamePlayerOne.Should().Be("Player 4");
                aggregateFromRepository.NamePlayerTwo.Should().Be(command.PlayerTwoName);

                _fixture.EventStore.GetSnapshotCalled.Should().BeTrue();
            }
        }
        
        private async void DoDispatch(ILifetimeScope scope, ICommand command)
        {
            var commandDispatcher = scope.Resolve<ICommandDispatcher>();

            await commandDispatcher.DispatchAsync(command).ConfigureAwait(false);
        }
    }
}