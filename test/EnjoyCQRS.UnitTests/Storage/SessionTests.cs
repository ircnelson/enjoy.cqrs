using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.UnitTests.Shared;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;
using FluentAssertions;
using Moq;
using Xunit;
using EnjoyCQRS.Stores;
using EnjoyCQRS.Stores.InMemory;

namespace EnjoyCQRS.UnitTests.Storage
{
    [Trait("Unit", "Session")]
    public class SessionTests
    {
        private readonly Func<ITransaction, IEventStore, ISnapshotStore, IEventPublisher, ISnapshotStrategy, EventsMetadataService, Session> _sessionFactory = (transaction, eventStore, snapshotStore, eventPublisher, snapshotStrategy, eventsMetadataService) =>
        {
            var session = new Session(MockHelper.CreateLoggerFactory(MockHelper.GetMockLogger().Object), transaction, eventStore, snapshotStore, eventPublisher, null, null, snapshotStrategy, eventsMetadataService);

            return session;
        };

        private readonly Mock<IEventPublisher> _eventPublisherMock;

        public SessionTests()
        {
            _eventPublisherMock = new Mock<IEventPublisher>();
        }

        
        [Fact]
        public void Cannot_pass_null_instance_of_LoggerFactory()
        {
            var eventPublisher = Mock.Of<IEventPublisher>();
            var transaction = Mock.Of<ITransaction>();
            var eventStore = Mock.Of<IEventStore>();
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var eventUpdateManager = Mock.Of<IEventUpdateManager>();
            var metadataProviders = Mock.Of<IEnumerable<IMetadataProvider>>();

            Action act = () => new Session(null, transaction, eventStore, snapshotStore, eventPublisher, eventUpdateManager, metadataProviders);

            act.ShouldThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void Cannot_pass_null_instance_of_EventStore()
        {
            var transaction = Mock.Of<ITransaction>();
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var eventPublisher = Mock.Of<IEventPublisher>();
            var eventUpdateManager = Mock.Of<IEventUpdateManager>();
            var metadataProviders = Mock.Of<IEnumerable<IMetadataProvider>>();

            Action act = () => new Session(MockHelper.CreateLoggerFactory(MockHelper.GetMockLogger().Object), transaction, null, snapshotStore, eventPublisher, eventUpdateManager, metadataProviders);

            act.ShouldThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void Cannot_pass_null_instance_of_EventPublisher()
        {
            var transaction = Mock.Of<ITransaction>();
            var eventStore = Mock.Of<IEventStore>();
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var eventUpdateManager = Mock.Of<IEventUpdateManager>();
            var metadataProviders = Mock.Of<IEnumerable<IMetadataProvider>>();

            Action act = () => new Session(MockHelper.CreateLoggerFactory(MockHelper.GetMockLogger().Object), transaction, eventStore, snapshotStore, null, eventUpdateManager, metadataProviders);

            act.ShouldThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public async Task Should_throws_exception_When_aggregate_version_is_wrong()
        {
            var stores = new InMemoryStores();

            // create first session instance
            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, null, null);

            var stubAggregate1 = StubAggregate.Create("Walter White");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            stubAggregate1.ChangeName("Going to Version 2. Expected Version 1.");

            // create second session instance to getting clear tracking
            session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, null, null);

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
            var stores = new InMemoryStores();

            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, null, null);

            var stubAggregate1 = StubAggregate.Create("Walter White");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            stubAggregate1.ChangeName("Changes");

            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id).ConfigureAwait(false);

            stubAggregate2.ChangeName("More changes");

            stubAggregate1.Should().BeSameAs(stubAggregate2);
        }
        
        [Fact]
        public async Task Should_publish_in_correct_order()
        {
            var events = new List<IDomainEvent>();

            var stores = new InMemoryStores();

            _eventPublisherMock.Setup(e => e.PublishAsync(It.IsAny<IEnumerable<IDomainEvent>>())).Callback<IEnumerable<IDomainEvent>>(evts => events.AddRange(evts)).Returns(Task.CompletedTask);

            _eventPublisherMock.Setup(e => e.CommitAsync()).Returns(Task.CompletedTask);

            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, null, null);

            var stubAggregate1 = StubAggregate.Create("Walter White");
            var stubAggregate2 = StubAggregate.Create("Heinsenberg");

            stubAggregate1.ChangeName("Saul Goodman");

            stubAggregate2.Relationship(stubAggregate1.Id);

            stubAggregate1.ChangeName("Jesse Pinkman");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);
            await session.AddAsync(stubAggregate2).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            events[0].Should().BeOfType<Event<StubAggregateCreatedEvent>>().Which.InnerEvent.AggregateId.Should().Be(stubAggregate1.Id);
            events[0].Should().BeOfType<Event<StubAggregateCreatedEvent>>().Which.InnerEvent.Name.Should().Be("Walter White");

            events[1].Should().BeOfType<Event<StubAggregateCreatedEvent>>().Which.InnerEvent.AggregateId.Should().Be(stubAggregate2.Id);
            events[1].Should().BeOfType<Event<StubAggregateCreatedEvent>>().Which.InnerEvent.Name.Should().Be("Heinsenberg");

            events[2].Should().BeOfType<Event<NameChangedEvent>>().Which.InnerEvent.AggregateId.Should().Be(stubAggregate1.Id);
            events[2].Should().BeOfType<Event<NameChangedEvent>>().Which.InnerEvent.Name.Should().Be("Saul Goodman");

            events[3].Should().BeOfType<Event<StubAggregateRelatedEvent>>().Which.InnerEvent.AggregateId.Should().Be(stubAggregate2.Id);
            events[3].Should().BeOfType<Event<StubAggregateRelatedEvent>>().Which.InnerEvent.StubAggregateId.Should().Be(stubAggregate1.Id);

            events[4].Should().BeOfType<Event<NameChangedEvent>>().Which.InnerEvent.AggregateId.Should().Be(stubAggregate1.Id);
            events[4].Should().BeOfType<Event<NameChangedEvent>>().Which.InnerEvent.Name.Should().Be("Jesse Pinkman");
        }
        
        [Fact]
        public async Task When_call_SaveChanges_Should_store_the_snapshot()
        {
            // Arrange

            var snapshotStrategy = CreateSnapshotStrategy();

            var stores = new InMemoryStores();
            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, null);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);

            // Act

            await session.SaveChangesAsync().ConfigureAwait(false);

            // Assert

            var committedSnapshot = stores.Snapshots.First(e => e.AggregateId == stubAggregate.Id);

            committedSnapshot.Should().NotBeNull();
            
            var snapshotClrType = committedSnapshot.Metadata.GetValue(MetadataKeys.SnapshotClrType, value => value.ToString());

            Type.GetType(snapshotClrType).Name.Should().Be(typeof(StubSnapshotAggregateSnapshot).Name);

            var snapshot = (StubSnapshotAggregateSnapshot) committedSnapshot.Data;

            snapshot.Name.Should().Be(stubAggregate.Name);
            snapshot.SimpleEntities.Count.Should().Be(stubAggregate.Entities.Count);
        }
        
        [Fact]
        public async Task Should_restore_aggregate_using_snapshot()
        {
            var snapshotStrategy = CreateSnapshotStrategy();

            var stores = new InMemoryStores();

            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, null);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, null);

            var aggregate = await session.GetByIdAsync<StubSnapshotAggregate>(stubAggregate.Id).ConfigureAwait(false);
            
            aggregate.Version.Should().Be(3);
            aggregate.Id.Should().Be(stubAggregate.Id);
        }

        [Fact]
        public async Task When_not_exists_snapshot_yet_Then_aggregate_should_be_constructed_using_your_events()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var stores = new InMemoryStores();

            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, null);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, null);

            var aggregate = await session.GetByIdAsync<StubSnapshotAggregate>(stubAggregate.Id).ConfigureAwait(false);

            aggregate.Name.Should().Be(stubAggregate.Name);
            aggregate.Entities.Count.Should().Be(stubAggregate.Entities.Count);
        }

        [Fact]
        public async Task Getting_snapshot_and_forward_events()
        {
            var snapshotStrategy = CreateSnapshotStrategy();

            var stores = new InMemoryStores();

            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, null);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false); // Version 1

            stubAggregate.ChangeName("Renamed");
            stubAggregate.ChangeName("Renamed again");

            // dont make snapshot
            snapshotStrategy = CreateSnapshotStrategy(false);

            session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, null);

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false); // Version 3

            var stubAggregateFromSnapshot = await session.GetByIdAsync<StubSnapshotAggregate>(stubAggregate.Id).ConfigureAwait(false);

            stubAggregateFromSnapshot.Version.Should().Be(3);
        }

        [Fact]
        public Task Should_throws_exception_When_aggregate_was_not_found()
        {
            var snapshotStrategy = CreateSnapshotStrategy();

            var stores = new InMemoryStores();

            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, null);

            var newId = Guid.NewGuid();

            Func<Task> act = async () =>
            {
                await session.GetByIdAsync<StubAggregate>(newId).ConfigureAwait(false);
            };

            var assertion = act.ShouldThrowExactly<AggregateNotFoundException>();

            assertion.Which.AggregateName.Should().Be(typeof(StubAggregate).Name);
            assertion.Which.AggregateId.Should().Be(newId);

            return Task.CompletedTask;
        }

        [Fact]
        public async Task Should_internal_rollback_When_exception_was_throw_on_saving()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var transactionMock = new Mock<ITransaction>();

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.Setup(e => e.AppendAsync(It.IsAny<IEnumerable<IUncommittedEvent>>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            var snapshotStoreMock = new Mock<ISnapshotStore>();

            var session = _sessionFactory(transactionMock.Object, eventStoreMock.Object, snapshotStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy, null);

            var stubAggregate = StubAggregate.Create("Guilty");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);

            try
            {
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                transactionMock.Verify(e => e.Rollback(), Times.Once);
                _eventPublisherMock.Verify(e => e.Rollback(), Times.Once);
            }
        }

        [Fact]
        public async Task Should_manual_rollback_When_exception_was_throw_on_saving()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var transactionMock = new Mock<ITransaction>();
            transactionMock.Setup(e => e.BeginTransaction());
            transactionMock.Setup(e => e.Rollback());

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.Setup(e => e.AppendAsync(It.IsAny<IEnumerable<IUncommittedEvent>>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            var snapshotStoreMock = new Mock<ISnapshotStore>();

            var session = _sessionFactory(transactionMock.Object, eventStoreMock.Object, snapshotStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy, null);

            var stubAggregate = StubAggregate.Create("Guilty");

            session.BeginTransaction();

            transactionMock.Verify(e => e.BeginTransaction(), Times.Once);

            await session.AddAsync(stubAggregate).ConfigureAwait(false);

            try
            {
                await session.SaveChangesAsync().ConfigureAwait(false);
            }

            catch (Exception)
            {
                session.Rollback();
            }

            transactionMock.Verify(e => e.Rollback(), Times.Once);
            _eventPublisherMock.Verify(e => e.Rollback(), Times.Once);
        }

        [Fact]
        public void Cannot_BeginTransaction_twice()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var transactionMock = new Mock<ITransaction>();
            transactionMock.SetupAllProperties();

            var eventStoreMock = new Mock<IEventStore>();
            var snapshotStoreMock = new Mock<ISnapshotStore>();

            var session = _sessionFactory(transactionMock.Object, eventStoreMock.Object, snapshotStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy, null);

            session.BeginTransaction();

            Action act = () => session.BeginTransaction();

            act.ShouldThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public async Task When_occur_error_on_publishing_Then_rollback_should_be_called()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var transactionMock = new Mock<ITransaction>();
            transactionMock.SetupAllProperties();

            _eventPublisherMock.Setup(e => e.PublishAsync(It.IsAny<IDomainEvent>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            _eventPublisherMock.Setup(e => e.PublishAsync(It.IsAny<IEnumerable<IDomainEvent>>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.SetupAllProperties();

            var snapshotStoreMock = new Mock<ISnapshotStore>();

            var session = _sessionFactory(transactionMock.Object, eventStoreMock.Object, snapshotStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy, null);

            await session.AddAsync(StubAggregate.Create("Test"));

            try
            {
                await session.SaveChangesAsync();
            }
            catch (Exception)
            {
                transactionMock.Verify(e => e.Rollback(), Times.Once);
                _eventPublisherMock.Verify(e => e.Rollback(), Times.Once);
            }
        }

        [Fact]
        public async Task When_occur_error_on_Save_in_EventStore_Then_rollback_should_be_called()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var transactionMock = new Mock<ITransaction>();
            transactionMock.SetupAllProperties();

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.SetupAllProperties();
            eventStoreMock.Setup(e => e.AppendAsync(It.IsAny<IEnumerable<IUncommittedEvent>>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            var snapshotStoreMock = new Mock<ISnapshotStore>();

            var session = _sessionFactory(transactionMock.Object, eventStoreMock.Object, snapshotStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy, null);

            await session.AddAsync(StubAggregate.Create("Test")).ConfigureAwait(false);

            try
            {
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                transactionMock.Verify(e => e.Rollback(), Times.Once);
                _eventPublisherMock.Verify(e => e.Rollback(), Times.Once);
            }
        }
        
        [Fact]
        public async Task Access_metadata_from_all_emitted_events()
        {
            // Arrange

            var snapshotStrategy = CreateSnapshotStrategy();

            var eventsMetadataService = new EventsMetadataService();

            var stores = new InMemoryStores();
            var session = _sessionFactory(stores, stores.EventStore, stores.SnapshotStore, _eventPublisherMock.Object, snapshotStrategy, eventsMetadataService);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);

            // Act

            await session.SaveChangesAsync().ConfigureAwait(false);

            // Assert

            var eventsWithMetadata = eventsMetadataService.GetEvents().ToList();

            eventsWithMetadata.Count().Should().Be(3);

            eventsWithMetadata[0].Metadata.GetValue(MetadataKeys.EventName).Should().Be("StubCreated");
            eventsWithMetadata[1].Metadata.GetValue(MetadataKeys.EventName).Should().Be(nameof(ChildCreatedEvent));
            eventsWithMetadata[2].Metadata.GetValue(MetadataKeys.EventName).Should().Be(nameof(ChildCreatedEvent));
        }

        private static ISnapshotStrategy CreateSnapshotStrategy(bool makeSnapshot = true)
        {
            var snapshotStrategyMock = new Mock<ISnapshotStrategy>();
            snapshotStrategyMock.Setup(e => e.CheckSnapshotSupport(It.IsAny<Type>())).Returns(true);
            snapshotStrategyMock.Setup(e => e.ShouldMakeSnapshot(It.IsAny<IAggregate>())).Returns(makeSnapshot);

            return snapshotStrategyMock.Object;
        }
        
        private static void DoThrowExcetion()
        {
            throw new Exception("Sorry, this is my fault.");
        }
    }
}