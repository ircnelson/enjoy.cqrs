using FluentAssertions;
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

        public DomainRepositoryTests()
        {
            var aggregateCache = new AggregateCache();
            _eventStoreUnitOfWork = new EventStoreUnitOfWork(aggregateCache, _inMemoryDomainEventStore);
            _domainRepository = new DomainRepository(_eventStoreUnitOfWork, aggregateCache);
        }

        [Fact]
        public void When_calling_Save_it_will_add_the_domain_events_to_the_domain_event_storage()
        {
            var testAggregate = TestAggregateRoot.Create();
            testAggregate.DoSomething("Heinsenberg");

            _domainRepository.Add(testAggregate);
            _eventStoreUnitOfWork.Commit();

            _inMemoryDomainEventStore.EventStore.Count.Should().Be(1);
        }
    }
}