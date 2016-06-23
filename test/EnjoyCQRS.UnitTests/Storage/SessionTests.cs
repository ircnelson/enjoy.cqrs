using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Shared;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.MetadataProviders;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class SessionTests
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Session";

        private readonly Func<IEventStore, IEventPublisher, ISnapshotStrategy, Session> _sessionFactory = (eventStore, eventPublisher, snapshotStrategy) =>
        {
            var eventSerializer = CreateEventSerializer();

            var session = new Session(CreateLoggerFactory(CreateLoggerMock().Object), eventStore, eventPublisher, eventSerializer, null, snapshotStrategy);

            return session;
        };
        
        private readonly Mock<IEventPublisher> _eventPublisherMock;

        public SessionTests()
        {
            _eventPublisherMock = new Mock<IEventPublisher>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Cannot_pass_null_instance_of_LoggerFactory()
        {
            var eventPublisher = Mock.Of<IEventPublisher>();
            var eventStore = Mock.Of<IEventStore>();
            var eventSerializer = Mock.Of<IEventSerializer>();
            var metadataProviders = Mock.Of<IEnumerable<IMetadataProvider>>();

            Action act = () => new Session(null, eventStore, eventPublisher, eventSerializer, metadataProviders);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Cannot_pass_null_instance_of_EventStore()
        {
            var eventSerializer = Mock.Of<IEventSerializer>();
            var eventPublisher = Mock.Of<IEventPublisher>();
            var metadataProviders = Mock.Of<IEnumerable<IMetadataProvider>>();

            Action act = () => new Session(CreateLoggerFactory(CreateLoggerMock().Object), null, eventPublisher, eventSerializer, metadataProviders);

            act.ShouldThrowExactly<ArgumentNullException>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Cannot_pass_null_instance_of_EventPublisher()
        {
            var eventStore = Mock.Of<IEventStore>();
            var eventSerializer = Mock.Of<IEventSerializer>();
            var metadataProviders = Mock.Of<IEnumerable<IMetadataProvider>>();

            Action act = () => new Session(CreateLoggerFactory(CreateLoggerMock().Object), eventStore, null, eventSerializer, metadataProviders);

            act.ShouldThrowExactly<ArgumentNullException>();
        }
        
        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Should_throws_exception_When_aggregate_version_is_wrong()
        {
            var eventStore = new InMemoryEventStore();

            // create first session instance
            var session = _sessionFactory(eventStore, _eventPublisherMock.Object, null);

            var stubAggregate1 = StubAggregate.Create("Walter White");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            stubAggregate1.ChangeName("Going to Version 2. Expected Version 1.");

            // create second session instance to getting clear tracking
            session = _sessionFactory(eventStore, _eventPublisherMock.Object, null);

            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id).ConfigureAwait(false);

            stubAggregate2.ChangeName("Going to Version 2");

            await session.AddAsync(stubAggregate2).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            Func<Task> wrongVersion = async () => await session.AddAsync(stubAggregate1);

            wrongVersion.ShouldThrowExactly<ExpectedVersionException<StubAggregate>>().And.Aggregate.Should().Be(stubAggregate1);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Should_retrieve_the_aggregate_from_tracking()
        {
            var eventStore = new InMemoryEventStore();

            var session = _sessionFactory(eventStore, _eventPublisherMock.Object, null);

            var stubAggregate1 = StubAggregate.Create("Walter White");

            await session.AddAsync(stubAggregate1).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            stubAggregate1.ChangeName("Changes");

            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id).ConfigureAwait(false);

            stubAggregate2.ChangeName("More changes");

            stubAggregate1.Should().BeSameAs(stubAggregate2);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task When_call_SaveChanges_Should_store_the_snapshot()
        {
            // Arrange

            var snapshotStrategy = CreateSnapshotStrategy();
            
            var eventStore = new StubEventStore();
            var session = _sessionFactory(eventStore, _eventPublisherMock.Object, snapshotStrategy);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);


            // Act

            await session.SaveChangesAsync().ConfigureAwait(false);
            
            // Assert

            eventStore.SaveSnapshotMethodCalled.Should().BeTrue();

            eventStore.Snapshots.First(e => e.AggregateId == stubAggregate.Id).Should().BeOfType<StubSnapshotAggregateSnapshot>();

            var snapshot = eventStore.Snapshots.First(e => e.AggregateId == stubAggregate.Id).As<StubSnapshotAggregateSnapshot>();

            snapshot.AggregateId.Should().Be(stubAggregate.Id);
            snapshot.Name.Should().Be(stubAggregate.Name);
            snapshot.SimpleEntities.Count.Should().Be(stubAggregate.Entities.Count);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Should_restore_aggregate_using_snapshot()
        {
            var snapshotStrategy = CreateSnapshotStrategy();
            
            var eventStore = new StubEventStore();

            var session = _sessionFactory(eventStore, _eventPublisherMock.Object, snapshotStrategy);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            session = _sessionFactory(eventStore, _eventPublisherMock.Object, snapshotStrategy);

            var aggregate = await session.GetByIdAsync<StubSnapshotAggregate>(stubAggregate.Id).ConfigureAwait(false);

            eventStore.GetSnapshotMethodCalled.Should().BeTrue();

            aggregate.Version.Should().Be(3);
            aggregate.Id.Should().Be(stubAggregate.Id);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public async Task When_not_exists_snapshot_yet_Then_aggregate_should_be_constructed_using_your_events()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);
            
            var eventStore = new StubEventStore();

            var session = _sessionFactory(eventStore, _eventPublisherMock.Object, snapshotStrategy);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            stubAggregate.AddEntity("Child 1");
            stubAggregate.AddEntity("Child 2");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            session = _sessionFactory(eventStore, _eventPublisherMock.Object, snapshotStrategy);

            var aggregate = await session.GetByIdAsync<StubSnapshotAggregate>(stubAggregate.Id).ConfigureAwait(false);

            aggregate.Name.Should().Be(stubAggregate.Name);
            aggregate.Entities.Count.Should().Be(stubAggregate.Entities.Count);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Getting_snapshot_and_forward_events()
        {
            var snapshotStrategy = CreateSnapshotStrategy();
            
            var eventStore = new StubEventStore();

            var session = _sessionFactory(eventStore, _eventPublisherMock.Object, snapshotStrategy);

            var stubAggregate = StubSnapshotAggregate.Create("Snap");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false); // Version 1

            stubAggregate.ChangeName("Renamed");
            stubAggregate.ChangeName("Renamed again");

            // dont make snapshot
            snapshotStrategy = CreateSnapshotStrategy(false);

            session = _sessionFactory(eventStore, _eventPublisherMock.Object, snapshotStrategy);

            await session.AddAsync(stubAggregate).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false); // Version 3
            
            var stubAggregateFromSnapshot = await session.GetByIdAsync<StubSnapshotAggregate>(stubAggregate.Id).ConfigureAwait(false);

            stubAggregateFromSnapshot.Version.Should().Be(3);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public Task Should_throws_exception_When_aggregate_was_not_found()
        {
            var snapshotStrategy = CreateSnapshotStrategy();
            
            var eventStore = new StubEventStore();

            var session = _sessionFactory(eventStore, _eventPublisherMock.Object, snapshotStrategy);

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

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Should_internal_rollback_When_exception_was_throw_on_saving()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.Setup(e => e.SaveAsync(It.IsAny<IEnumerable<ISerializedEvent>>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            var session = _sessionFactory(eventStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy);

            var stubAggregate = StubAggregate.Create("Guilty");

            await session.AddAsync(stubAggregate).ConfigureAwait(false);

            try
            {
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                eventStoreMock.Verify(e => e.Rollback(), Times.Once);
                _eventPublisherMock.Verify(e => e.Rollback(), Times.Once);
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Should_manual_rollback_When_exception_was_throw_on_saving()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.Setup(e => e.SaveAsync(It.IsAny<IEnumerable<ISerializedEvent>>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            eventStoreMock.Setup(e => e.BeginTransaction());
            eventStoreMock.Setup(e => e.Rollback());

            var session = _sessionFactory(eventStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy);

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
            _eventPublisherMock.Verify(e => e.Rollback(), Times.Once);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Cannot_BeginTransaction_twice()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.SetupAllProperties();

            var session = _sessionFactory(eventStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy);

            session.BeginTransaction();

            Action act = () => session.BeginTransaction();

            act.ShouldThrowExactly<InvalidOperationException>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task When_occur_error_on_publishing_Then_rollback_should_be_called()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);

            _eventPublisherMock.Setup(e => e.PublishAsync(It.IsAny<IDomainEvent>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            _eventPublisherMock.Setup(e =>e.PublishAsync(It.IsAny<IEnumerable<IDomainEvent>>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.SetupAllProperties();

            var session = _sessionFactory(eventStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy);

            await session.AddAsync(StubAggregate.Create("Test"));
            
            try
            {
                await session.SaveChangesAsync();
            }
            catch (Exception)
            {
                eventStoreMock.Verify(e => e.Rollback(), Times.Once);
                _eventPublisherMock.Verify(e => e.Rollback(), Times.Once);
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task When_occur_error_on_Save_in_EventStore_Then_rollback_should_be_called()
        {
            var snapshotStrategy = CreateSnapshotStrategy(false);
            
            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.SetupAllProperties();
            eventStoreMock.Setup(e => e.SaveAsync(It.IsAny<IEnumerable<ISerializedEvent>>()))
                .Callback(DoThrowExcetion)
                .Returns(Task.CompletedTask);

            var session = _sessionFactory(eventStoreMock.Object, _eventPublisherMock.Object, snapshotStrategy);

            await session.AddAsync(StubAggregate.Create("Test")).ConfigureAwait(false);

            try
            {
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                eventStoreMock.Verify(e => e.Rollback(), Times.Once);
                _eventPublisherMock.Verify(e => e.Rollback(), Times.Once);
            }
        }

        private static ISnapshotStrategy CreateSnapshotStrategy(bool makeSnapshot = true)
        {
            var snapshotStrategyMock = new Mock<ISnapshotStrategy>();
            snapshotStrategyMock.Setup(e => e.CheckSnapshotSupport(It.IsAny<Type>())).Returns(true);
            snapshotStrategyMock.Setup(e => e.ShouldMakeSnapshot(It.IsAny<IAggregate>())).Returns(makeSnapshot);

            return snapshotStrategyMock.Object;
        }

        private static Mock<ILogger> CreateLoggerMock()
        {
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(e => e.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            mockLogger.Setup(e => e.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()));

            return mockLogger;
        }

        private static ILoggerFactory CreateLoggerFactory(ILogger logger)
        {
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(e => e.Create(It.IsAny<string>())).Returns(logger);

            return mockLoggerFactory.Object;
        }

        private static IEventSerializer CreateEventSerializer()
        {
            return new EventSerializer(new JsonTextSerializer());
        }
        
        private static void DoThrowExcetion()
        {
            throw new Exception("Sorry, this is my fault.");
        }
    }
}