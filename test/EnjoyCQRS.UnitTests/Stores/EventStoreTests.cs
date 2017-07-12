using EnjoyCQRS.Stores;
using System;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.UnitTests.Shared.Projection;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Stores
{
    public class EventStoreTests
    {
        [Fact]
        public async Task Should_retrieve_all_events_by_specific_aggregate()
        {
            var store = new System.Collections.Concurrent.ConcurrentBag<EnvelopedEvent>();

            IEventStore eventStore = new MemoryEventStore(store);

            var writer = eventStore.GetWriter<Guid>();
            var reader = eventStore.GetReader<Guid>();

            var user1 = Guid.NewGuid();
            var user2 = Guid.NewGuid();

            var events = new DomainEvent[]
            {
                new UserCreated(user1, DateTime.Now, "Walter", "White", DateTime.Today),
                new UserCreated(user2, DateTime.Now, "Walter", "White", DateTime.Today)
            };

            var uncommitedEvents = events.Select(e => new UncommitedDomainEvent(e.AggregateId, e));

            await writer.AppendAsync(uncommitedEvents);

            var allEvents = await reader.GetAllAsync(user1);

            allEvents.Count().Should().Be(1);
        }
    }
}
