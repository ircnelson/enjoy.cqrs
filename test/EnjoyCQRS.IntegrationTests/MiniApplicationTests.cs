using System;
using System.Threading.Tasks;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Fixtures;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.BarAggregate;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FooAggregate;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.BarAggregate;
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
        
        [Trait(CategoryName, CategoryValue)]
        [Fact]
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

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Should_take_and_restore_snapshot_based_on_interval_strategy_configured()
        {
            const int times = 5;

            _fixture.SnapshotStrategy = new IntervalSnapshotStrategy(times);

            var aggregateId = Guid.NewGuid();

            var command = new CreateFooCommand(aggregateId);

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                DoDispatch(scope, command);

                _fixture.EventStore.SaveSnapshotCalled.Should().BeFalse();
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                DoDispatch(scope, new DoFloodSomethingCommand(aggregateId, times));
                
                _fixture.EventStore.SaveSnapshotCalled.Should().BeTrue();
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var repository = scope.Resolve<IRepository>();

                var aggregateFromRepository = await repository.GetByIdAsync<Foo>(aggregateId).ConfigureAwait(false);

                aggregateFromRepository.Should().NotBeNull();

                aggregateFromRepository.Id.Should().Be(command.AggregateId);

                // (times - 1) because already emitted create event...
                aggregateFromRepository.DidSomethingCounter.Should().Be(times-1);

                _fixture.EventStore.GetSnapshotCalled.Should().BeTrue();
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Should_get_all_events()
        {
            _fixture.SnapshotStrategy = new IntervalSnapshotStrategy(200);

            var aggregateId = Guid.NewGuid();
            
            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                DoDispatch(scope, new CreateBarCommand(aggregateId));
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                DoDispatch(scope, new SpeakCommand(aggregateId, "e N J o y"));
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var repository = scope.Resolve<IRepository>();

                var aggregateFromRepository = await repository.GetByIdAsync<Bar>(aggregateId).ConfigureAwait(false);

                aggregateFromRepository.LastText.Should().Be("e N J o y");
            }
        }

        private async void DoDispatch(ILifetimeScope scope, ICommand command)
        {
            var commandDispatcher = scope.Resolve<ICommandDispatcher>();

            await commandDispatcher.DispatchAsync(command).ConfigureAwait(false);
        }
    }
}