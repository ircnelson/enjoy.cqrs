using System;
using System.Collections.Generic;
using EnjoyCQRS.Bus;
using EnjoyCQRS.EventStore;
using EnjoyCQRS.EventStore.Exceptions;
using EnjoyCQRS.EventStore.Storage;
using EnjoyCQRS.UnitTests.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class SessionTests
    {
        private readonly StubEventStore _inMemoryDomainEventStore = new StubEventStore();
        private readonly Session _session;
        private readonly IRepository _domainRepository;
        private readonly Mock<IMessageBus> _mockMessageBus;

        public SessionTests()
        {
            _mockMessageBus = new Mock<IMessageBus>();
            _mockMessageBus.Setup(e => e.Publish(It.IsAny<object>()));
            
            _domainRepository = new Repository(_inMemoryDomainEventStore, _mockMessageBus.Object);
            _session = new Session(_domainRepository, _mockMessageBus.Object);
        }

        [Fact]
        public void When_calling_Save_it_will_add_the_domain_events_to_the_domain_event_storage()
        {
            var testAggregate = TestAggregateRoot.Create();
            testAggregate.DoSomething("Heisenberg");

            _session.Add(testAggregate);
            _session.Commit();

            _inMemoryDomainEventStore.EventStore.Count.Should().Be(2);
        }

        [Fact]
        public void When_calling_Save_the_uncommited_events_should_be_published()
        {
            var testAggregate = TestAggregateRoot.Create();
            testAggregate.DoSomething("Heisenberg");

            _session.Add(testAggregate);
            _session.Commit();

            _mockMessageBus.Verify(e => e.Publish(It.IsAny<IEnumerable<object>>()), Times.Once);
        }

        [Fact]
        public void When_load_aggregate_should_be_correct_version()
        {
            var testAggregate = TestAggregateRoot.Create();
            testAggregate.DoSomething("Heisenberg");

            _session.Add(testAggregate);
            _session.Commit();

            var testAggregate2 = _domainRepository.GetById<TestAggregateRoot>(testAggregate.Id);
            
            testAggregate.Version.Should().Be(testAggregate2.Version);
        }

        //TODO: fix it!

        //[Fact]
        //public void When_try_save_aggregate_out_of_date_Should_throws_a_ConcurrentException()
        //{
        //    var testAggregate = TestAggregateRoot.Create();
        //    testAggregate.DoSomething("Walter White");

        //    _domainRepository.Save(testAggregate);
        //    _eventStoreUnitOfWork.Commit();
            
        //    // To dont keep reference, the previous aggregate instance was removed
        //    _aggregateTracker.Remove(testAggregate.GetType(), testAggregate.Id);

        //    var testAggregate2 = _domainRepository.GetById<TestAggregateRoot>(testAggregate.Id);

        //    Action action = () =>
        //    {
        //        testAggregate.DoSomething("Heisenberg");
        //        _domainRepository.Save(testAggregate);
        //        _eventStoreUnitOfWork.Commit();
                
        //        testAggregate2.DoSomething("Gus");
        //        _domainRepository.Save(testAggregate2);
        //        _eventStoreUnitOfWork.Commit();
        //    };

        //    action.ShouldThrow<WrongExpectedVersionException>();
        //}
    }
}