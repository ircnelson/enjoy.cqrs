using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FooAggregate;
using Newtonsoft.Json;

namespace EnjoyCQRS.IntegrationTests.Shared.TestSuit
{
    public class EventStoreTestSuit
    {
        private readonly IEventStore _eventStore;

        public EventStoreTestSuit(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task EventTestsAsync()
        {
            var aggregateId = Guid.NewGuid();
            var rand = new Random();

            var stubEvent = new FooCreated(Guid.NewGuid());

            var serializedEvents = new List<ISerializedEvent>();

            for (var i = 0; i < 10; i++)
            {
                serializedEvents.Add(CreateSerializedEvent(stubEvent, aggregateId, rand.Next(0, 5)));
            }

            _eventStore.BeginTransaction();

            await _eventStore.SaveAsync(serializedEvents);
            await _eventStore.CommitAsync();

            await _eventStore.GetAllEventsAsync(aggregateId);
        }

        public async Task SnapshotTestsAsync()
        {
            var foo = new Foo(Guid.NewGuid());
            for (var i = 0; i < 10; i++)
            {
                foo.DoSomething();
            }

            var snapshot = ((ISnapshotAggregate)foo).CreateSnapshot();

            var serializedEvents = foo.UncommitedEvents.Select((e, i) => CreateSerializedEvent(e, foo.Id, i));

            await _eventStore.SaveAsync(serializedEvents);
            await _eventStore.SaveSnapshotAsync(snapshot);
            await _eventStore.CommitAsync();

            snapshot = await _eventStore.GetLatestSnapshotByIdAsync(foo.Id);

            await _eventStore.GetEventsForwardAsync(foo.Id, snapshot.Version);
        }
        
        private ISerializedEvent CreateSerializedEvent(IDomainEvent @event, Guid aggregateId, int version)
        {
            var metadatas = new[]
            {
                new KeyValuePair<string, string>(MetadataKeys.EventId, Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>(MetadataKeys.EventName, @event.GetType().Name),
                new KeyValuePair<string, string>(MetadataKeys.EventVersion, version.ToString()),
                new KeyValuePair<string, string>(MetadataKeys.AggregateId, aggregateId.ToString())
            };

            var metadata = new Metadata(metadatas);
            
            var serializedEvent = JsonConvert.SerializeObject(@event);
            var serializedMetadata = JsonConvert.SerializeObject(metadata);

            return new SerializedEvent(aggregateId, 1, serializedEvent, serializedMetadata, new Metadata(metadata));
        }
    }
}