using System;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Collections;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class SessionTests
    {
        private readonly Func<IEventStore, ISnapshotStrategy, Session> _sessionFactory = (eventStore, snapshotStrategy) =>
        {
            var eventPublisherMock = new Mock<IEventPublisher>();
            var session = new Session(eventStore, eventPublisherMock.Object, snapshotStrategy);

            return session;
        };

        [Fact]
        public void Cannot_pass_null_instance_of_EventStore()
        {
            var eventPublisher = Mock.Of<IEventPublisher>();

            Action act = () => new Session(null, eventPublisher);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Cannot_pass_null_instance_of_EventPublisher()
        {
            var eventStore = Mock.Of<IEventStore>();
            
            Action act = () => new Session(eventStore, null);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public async Task Should_throws_exception_When_aggregate_version_is_wrong()
        {
            var eventStore = new StubEventStore();

            // create first session instance
            var session = _sessionFactory(eventStore, null);

            var stubAggregate1 = StubAggregate.Create("Walter White");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            stubAggregate1.ChangeName("Going to Version 2. Expected Version 1.");

            // create second session instance to getting clear tracking
            session = _sessionFactory(eventStore, null);

            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id).ConfigureAwait(false);

            stubAggregate2.ChangeName("Going to Version 2");

            await session.AddAsync(stubAggregate2).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            Func<Task> wrongVersion = async () => await session.AddAsync(stubAggregate1);

            wrongVersion.ShouldThrowExactly<ExpectedVersionException<StubAggregate>>().And.Aggregate.Should().Be(stubAggregate1);
        }

        [Fact]
        public async Task Should_retrieve_the_aggregate_from_tracking()
        {
            var eventStore = new StubEventStore();
            var session = _sessionFactory(eventStore, null);

            var stubAggregate1 = StubAggregate.Create("Walter White");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            stubAggregate1.ChangeName("Changes");

            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id).ConfigureAwait(false);

            stubAggregate2.ChangeName("More changes");

            stubAggregate1.Should().BeSameAs(stubAggregate2);
        }

        [Fact]
        public async Task When_call_SaveChanges_Should_store_the_snapshot()
        {
            // Arrange

            var snapshotStrategy = CreateSnapshotStrategy();

            var eventStore = new StubEventStore();
            var session = _sessionFactory(eventStore, snapshotStrategy);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);


            // Act

            await session.SaveChangesAsync().ConfigureAwait(false);


            // Assert

            eventStore.SaveSnapshotMethodCalled.Should().BeTrue();

            eventStore.SnapshotStore[stubAggregate.Id].First().Should().BeOfType<StubSnapshotAggregateSnapshot>();

            var snapshot = eventStore.SnapshotStore[stubAggregate.Id].First().As<StubSnapshotAggregateSnapshot>();

            snapshot.AggregateId.Should().Be(stubAggregate.Id);
            snapshot.Name.Should().Be(stubAggregate.Name);
            snapshot.SimpleEntities.Count.Should().Be(stubAggregate.Entities.Count);
        }

        [Fact]
        public async Task Should_restore_aggregate_using_snapshot()
        {
            var snapshotStrategy = CreateSnapshotStrategy();

            var eventStore = new StubEventStore();
            var session = _sessionFactory(eventStore, snapshotStrategy);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            session = _sessionFactory(eventStore, null);

            var aggregate = await session.GetByIdAsync<StubSnapshotAggregate>(stubAggregate.Id).ConfigureAwait(false);

            eventStore.GetSnapshotMethodCalled.Should().BeTrue();

            aggregate.Version.Should().Be(3);
            aggregate.Id.Should().Be(stubAggregate.Id);
        }


        [Fact]
        public async Task Getting_snapshot_and_forward_events()
        {
            var snapshotStrategy = CreateSnapshotStrategy();

            var eventStore = new StubEventStore();

            var session = _sessionFactory(eventStore, snapshotStrategy);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false); // Version 1

            stubAggregate.ChangeName("Renamed");
            stubAggregate.ChangeName("Renamed again");

            // dont make snapshot
            snapshotStrategy = CreateSnapshotStrategy(false);

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false); // Version 3

            session = _sessionFactory(eventStore, snapshotStrategy);

            var stubAggregateFromSnapshot = await session.GetByIdAsync<StubSnapshotAggregate>(stubAggregate.Id).ConfigureAwait(false);

            stubAggregateFromSnapshot.Version.Should().Be(3);
        }

        [Fact]
        public Task Should_throws_exception_When_aggregate_was_not_found()
        {
            var snapshotStrategy = CreateSnapshotStrategy();
            var eventStore = new StubEventStore();

            var session = _sessionFactory(eventStore, snapshotStrategy);

            Func<Task> act = async () =>
            {
                await session.GetByIdAsync<StubAggregate>(Guid.NewGuid()).ConfigureAwait(false);
            };

            act.ShouldThrowExactly<AggregateNotFoundException>();

            return Task.CompletedTask;
        }

        [Fact]
        public async Task Should_internal_rollback_When_exception_was_throw_on_saving()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.Setup(e => e.SaveAsync(It.IsAny<UncommitedDomainEventCollection>()))
                .Callback(() => { throw new Exception("Sorry, this is my fault."); })
                .Returns(Task.CompletedTask);

            var session = _sessionFactory(eventStoreMock.Object, snapshotStrategy);

            var stubAggregate = StubAggregate.Create("Guilty");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);

            try
            {
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                eventStoreMock.Verify(e => e.Rollback());
            }
        }

        [Fact]
        public async Task Should_manual_rollback_When_exception_was_throw_on_saving()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.Setup(e => e.SaveAsync(It.IsAny<UncommitedDomainEventCollection>()))
                .Callback(() => { throw new Exception("Sorry, this is my fault."); })
                .Returns(Task.CompletedTask);

            eventStoreMock.Setup(e => e.BeginTransaction());
            eventStoreMock.Setup(e => e.Rollback());

            var session = _sessionFactory(eventStoreMock.Object, snapshotStrategy);

            var stubAggregate = StubAggregate.Create("Guilty");

            session.BeginTransaction();

            eventStoreMock.Verify(e => e.BeginTransaction(), Times.Once);

            await session.AddAsync(stubAggregate).ConfigureAwait(false);

            try
            {
                await session.SaveChangesAsync().ConfigureAwait(false);
            }

            catch (Exception)
            {
                session.Rollback();
            }

            eventStoreMock.Verify(e => e.Rollback(), Times.Once);
        }

        [Fact]
        public void Cannot_BeginTransaction_twice()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.SetupAllProperties();

            var session = _sessionFactory(eventStoreMock.Object, snapshotStrategy);

            session.BeginTransaction();

            Action act = () => session.BeginTransaction();

            act.ShouldThrowExactly<InvalidOperationException>();
        }

        private static ISnapshotStrategy CreateSnapshotStrategy(bool makeSnapshot = true)
        {
            var snapshotStrategyMock = new Mock<ISnapshotStrategy>();
            snapshotStrategyMock.Setup(e => e.CheckSnapshotSupport(It.IsAny<Type>())).Returns(true);
            snapshotStrategyMock.Setup(e => e.ShouldMakeSnapshot(It.IsAny<IAggregate>())).Returns(makeSnapshot);

            return snapshotStrategyMock.Object;
        }
    }
}