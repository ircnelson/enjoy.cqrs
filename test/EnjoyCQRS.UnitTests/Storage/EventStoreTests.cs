using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.UnitTests.Domain;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class EventStoreTests
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Event store";

        private readonly StubEventStore _inMemoryDomainEventStore = new StubEventStore();
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository _repository;
        private readonly Mock<IEventPublisher> _mockEventPublisher;

        public EventStoreTests()
        {
            _mockEventPublisher = new Mock<IEventPublisher>();
            _mockEventPublisher.Setup(e => e.PublishAsync(It.IsAny<IEnumerable<IDomainEvent>>())).Returns(Task.CompletedTask);
            
            var session = new Session(_inMemoryDomainEventStore, _mockEventPublisher.Object);
            _repository = new Repository(session);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(e => e.CommitAsync())
                .Callback(async () =>
                {
                    await session.SaveChangesAsync().ConfigureAwait(false);
                })
                .Returns(Task.CompletedTask);

            _unitOfWork = unitOfWorkMock.Object;
        }

        [Then]
        [Trait(CategoryName, CategoryValue)]
        public async Task When_calling_Save_it_will_add_the_domain_events_to_the_domain_event_storage()
        {
            var testAggregate = StubAggregate.Create("Walter White");
            testAggregate.ChangeName("Heinsenberg");

            await _repository.AddAsync(testAggregate).ConfigureAwait(false);

            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _inMemoryDomainEventStore.EventStore[testAggregate.Id].Count.Should().Be(2);
        }

        [Then]
        [Trait(CategoryName, CategoryValue)]
        public async Task When_Save_Then_the_uncommited_events_should_be_published()
        {
            var testAggregate = StubAggregate.Create("Walter White");
            testAggregate.ChangeName("Heinsenberg");

            await _repository.AddAsync(testAggregate).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            _mockEventPublisher.Verify(e => e.PublishAsync(It.IsAny<IEnumerable<IDomainEvent>>()));
        }

        [Then]
        [Trait(CategoryName, CategoryValue)]
        public async Task When_load_aggregate_should_be_correct_version()
        {
            var testAggregate = StubAggregate.Create("Walter White");
            testAggregate.ChangeName("Heinsenberg");

            await _repository.AddAsync(testAggregate).ConfigureAwait(false);
            await _unitOfWork.CommitAsync().ConfigureAwait(false);

            var testAggregate2 = await _repository.GetByIdAsync<StubAggregate>(testAggregate.Id).ConfigureAwait(false);
            
            testAggregate.Version.Should().Be(testAggregate2.Version);
        }
    }
}