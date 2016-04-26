using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MyCQRS.Bus;
using MyCQRS.EventStore;
using MyCQRS.EventStore.Storage;
using MyCQRS.UnitTests.Domain;
using Xunit;

namespace MyCQRS.UnitTests.Storage
{
    public class DomainRepositoryTests
    {
        private readonly InMemoryDomainEventStore _inMemoryDomainEventStore = new InMemoryDomainEventStore();
        private readonly EventStoreUnitOfWork _eventStoreUnitOfWork;
        private readonly IDomainRepository _domainRepository;
        private readonly Mock<IMessageBus> _mockMessageBus;
        private AggregateCache _aggregateCache;

        public DomainRepositoryTests()
        {
            _mockMessageBus = new Mock<IMessageBus>();
            _mockMessageBus.Setup(e => e.Publish(It.IsAny<object>()));

            _aggregateCache = new AggregateCache();
            
            _eventStoreUnitOfWork = new EventStoreUnitOfWork(_aggregateCache, _inMemoryDomainEventStore, _mockMessageBus.Object);
            _domainRepository = new DomainRepository(_eventStoreUnitOfWork, _aggregateCache);
        }

        [Fact]
        public void When_calling_Save_it_will_add_the_domain_events_to_the_domain_event_storage()
        {
            var testAggregate = TestAggregateRoot.Create();
            testAggregate.DoSomething("Heisenberg");

            _domainRepository.Add(testAggregate);
            _eventStoreUnitOfWork.Commit();

            _inMemoryDomainEventStore.EventStore.Count.Should().Be(1);
        }

        [Fact]
        public void When_calling_Save_the_uncommited_events_should_be_published()
        {
            var testAggregate = TestAggregateRoot.Create();
            testAggregate.DoSomething("Heisenberg");

            _domainRepository.Add(testAggregate);
            _eventStoreUnitOfWork.Commit();

            _mockMessageBus.Verify(e => e.Publish(It.IsAny<IEnumerable<object>>()), Times.Once);
        }

        [Fact]
        public void When_load_aggregate_should_be_correct_version()
        {
            var testAggregate = TestAggregateRoot.Create();
            testAggregate.DoSomething("Heisenberg");

            _domainRepository.Add(testAggregate);
            _eventStoreUnitOfWork.Commit();

            var testAggregate2 = _domainRepository.GetById<TestAggregateRoot>(testAggregate.Id);
            
            testAggregate.Version.Should().Be(testAggregate2.Version);
        }
    }
}