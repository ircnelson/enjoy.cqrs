using System;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Messages;
using EnjoyCQRS.UnitTests.Domain;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class SessionTests
    {
        private readonly Func<IEventStore, Session> _sessionFactory = (eventStore) =>
        {
            var eventPublisherMock = new Mock<IEventPublisher>();
            var session = new Session(eventStore, eventPublisherMock.Object);

            return session;
        };

        [Fact]
        public async Task Should_throw_exception_When_aggregate_version_is_wrong()
        {
            var eventStore = new StubEventStore();

            // create first session instance
            var session = _sessionFactory(eventStore);

            var stubAggregate1 = StubAggregate.Create("Walter White");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            stubAggregate1.ChangeName("Going to Version 2. Expected Version 1.");

            // create second session instance to getting clear tracking
            session = _sessionFactory(eventStore);

            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id).ConfigureAwait(false);

            stubAggregate2.ChangeName("Going to Version 2");

            await session.AddAsync(stubAggregate2).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            Func<Task> wrongVersion = async () => await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            wrongVersion.ShouldThrow<ExpectedVersionException<StubAggregate>>()
                .And.Aggregate.Should().Be(stubAggregate1);
        }

        [Fact]
        public async Task Should_retrieve_the_aggregate_from_tracking()
        {
            var eventStore = new StubEventStore();
            var session = _sessionFactory(eventStore);

            var stubAggregate1 = StubAggregate.Create("Walter White");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            stubAggregate1.ChangeName("Changes");
            
            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id).ConfigureAwait(false);

            stubAggregate2.ChangeName("More changes");

            stubAggregate1.Should().BeSameAs(stubAggregate2);
        }

        [Fact]
        public async Task When_SaveChanges_method_called_Should_store_the_snapshot()
        {
            // Arrange

            var eventStore = new StubEventStore();
            var session = _sessionFactory(eventStore);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            

            // Act

            await session.SaveChangesAsync().ConfigureAwait(false);


            // Assert

            eventStore.SnapshotStore[stubAggregate.Id].First().Should().BeOfType<StubSnapshotAggregateSnapshot>();

            var snapshot = eventStore.SnapshotStore[stubAggregate.Id].First().As<StubSnapshotAggregateSnapshot>();

            snapshot.AggregateId.Should().Be(stubAggregate.Id);
            snapshot.Name.Should().Be(stubAggregate.Name);
            snapshot.SimpleEntities.Count.Should().Be(stubAggregate.Entities.Count);
        }
    }
}