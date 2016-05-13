using System;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Messages;
using EnjoyCQRS.UnitTests.Domain;
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

            var stubAggregate1 = StubAggregate.Create();

            await session.AddAsync(stubAggregate1);
            await session.SaveChangesAsync();

            stubAggregate1.DoSomething("Going to Version 2. Expected Version 1.");

            // create second session instance to getting clear tracking
            session = _sessionFactory(eventStore);

            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id);

            stubAggregate2.DoSomething("Going to Version 2");

            await session.AddAsync(stubAggregate2);
            await session.SaveChangesAsync();

            Func<Task> wrongVersion = async () =>
            {
                await session.AddAsync(stubAggregate1);
            };

            wrongVersion.ShouldThrow<ExpectedVersionException<StubAggregate>>()
                .And.Aggregate.Should().Be(stubAggregate1);
        }

        [Fact]
        public async Task Should_retrieve_the_aggregate_from_tracking()
        {
            var eventStore = new StubEventStore();
            var session = _sessionFactory(eventStore);

            var stubAggregate1 = StubAggregate.Create();

            await session.AddAsync(stubAggregate1);
            await session.SaveChangesAsync();

            stubAggregate1.DoSomething("Changes");
            
            var stubAggregate2 = await session.GetByIdAsync<StubAggregate>(stubAggregate1.Id);

            stubAggregate2.DoSomething("More changes");

            stubAggregate1.Should().BeSameAs(stubAggregate2);
        }
    }
}