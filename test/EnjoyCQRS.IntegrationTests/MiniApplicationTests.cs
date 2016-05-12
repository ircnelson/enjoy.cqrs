using System;
using System.Collections.Generic;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Fixtures;
using EnjoyCQRS.IntegrationTests.Stubs;
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
        public void Should_retrieves_the_aggregate_from_tracking()
        {
            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(Guid.NewGuid(), "Fake");

                DoDispatch(scope, command);
                
                var repository = scope.Resolve<IRepository>();
                var aggregateTracker = scope.Resolve<IAggregateTracker>();

                var aggregateFromTracking = aggregateTracker.GetById<FakePerson>(command.AggregateId);
                var aggregateFromRepository = repository.GetById<FakePerson>(command.AggregateId);
                
                aggregateFromTracking.Should().BeSameAs(aggregateFromRepository);
            }
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Aggregate_cannot_exists_in_tracking_on_another_scope()
        {
            var aggregateId = Guid.NewGuid();

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(aggregateId, "Fake");

                DoDispatch(scope, command);
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var aggregateTracker = scope.Resolve<IAggregateTracker>();
                var aggregateFromTracking = aggregateTracker.GetById<FakePerson>(aggregateId);

                aggregateFromTracking.Should().BeNull();
            }
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Should_retrieves_an_aggregate_from_EventStore_When_it_was_not_tracked()
        {
            var aggregateId = Guid.NewGuid();

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(aggregateId, "Fake");

                DoDispatch(scope, command);
            }

            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var aggregateTracker = scope.Resolve<IAggregateTracker>();
                var aggregateFromTracking = aggregateTracker.GetById<FakePerson>(aggregateId);

                aggregateFromTracking.Should().BeNull();
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