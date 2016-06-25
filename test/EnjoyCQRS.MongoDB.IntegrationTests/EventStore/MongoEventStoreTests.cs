using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventStore.MongoDB;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FooAggregate;
using FluentAssertions;
using MongoDB.Embedded;
using Newtonsoft.Json;
using Xunit;

namespace EnjoyCQRS.MongoDB.IntegrationTests.EventStore
{
    public class MongoEventStoreTests
    {
        public const string DatabaseName = "test";

        [Fact]
        public void Should_create_database()
        {
            using (EmbeddedMongoDbServer embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);

                var database = embeddedMongoDbServer.Client.GetDatabase(DatabaseName);

                database.Should().NotBeNull();

                eventStore.Database.Should().Be(DatabaseName);
            }
        }

        [Fact]
        public void Should_case_an_exception_when_MetadataKey_not_found()
        {
            var stubEvent = new FooCreated(Guid.NewGuid());

            var metadatas = new[]
            {
                new KeyValuePair<string, string>(MetadataKeys.AggregateId, Guid.NewGuid().ToString())
            };

            ISerializedEvent[] serializedEvents = {
                CreateSerializedEvent(stubEvent, metadatas)
            };

            using (EmbeddedMongoDbServer embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);
                
                Func<Task> action = async () => await eventStore.SaveAsync(serializedEvents);

                action.ShouldThrowExactly<KeyNotFoundException>();
            }
        }

        [Fact]
        public async Task Should_save_many_events()
        {
            var rand = new Random();

            var aggregateId = Guid.NewGuid();

            var stubEvent = new FooCreated(Guid.NewGuid());
            
            var serializedEvents = new List<ISerializedEvent>();

            for (var i = 0; i < 10; i++)
            {
                var metadata = new Metadata(new[]
                {
                    new KeyValuePair<string, string>(MetadataKeys.EventId, Guid.NewGuid().ToString()),
                    new KeyValuePair<string, string>(MetadataKeys.EventName, stubEvent.GetType().Name),
                    new KeyValuePair<string, string>(MetadataKeys.EventVersion, rand.Next(0, 5).ToString()),
                    new KeyValuePair<string, string>(MetadataKeys.AggregateId, aggregateId.ToString())
                });

                serializedEvents.Add(CreateSerializedEvent(stubEvent, metadata));
            }

            using (EmbeddedMongoDbServer embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);
                eventStore.BeginTransaction();

                await eventStore.SaveAsync(serializedEvents);

                await eventStore.CommitAsync();
            }
        }

        [Fact]
        public async Task Should_get_events()
        {
            const int numberOfEvents = 10;
            
            var aggregateId = Guid.NewGuid();

            var stubEvent = new FooCreated(Guid.NewGuid());

            var serializedEvents = new List<ISerializedEvent>();

            for (var i = 0; i < numberOfEvents; i++)
            {
                var metadata = new Metadata(new[]
                {
                    new KeyValuePair<string, string>(MetadataKeys.EventId, Guid.NewGuid().ToString()),
                    new KeyValuePair<string, string>(MetadataKeys.EventName, stubEvent.GetType().Name),
                    new KeyValuePair<string, string>(MetadataKeys.EventVersion, i.ToString()),
                    new KeyValuePair<string, string>(MetadataKeys.AggregateId, aggregateId.ToString())
                });

                serializedEvents.Add(CreateSerializedEvent(stubEvent, metadata));
            }

            IEnumerable<ICommitedEvent> commitedEvents;

            using (EmbeddedMongoDbServer embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);
                
                await eventStore.SaveAsync(serializedEvents);
                await eventStore.CommitAsync();

                commitedEvents = await eventStore.GetAllEventsAsync(aggregateId);
            }

            commitedEvents.Should().NotBeNull();
            commitedEvents.Count().Should().Be(numberOfEvents);
        }

        private ISerializedEvent CreateSerializedEvent(IDomainEvent @event, IEnumerable<KeyValuePair<string, string>> metadatas)
        {
            var metadata = new Metadata(metadatas);

            var aggregateId = metadata.GetValue(MetadataKeys.AggregateId, Guid.Parse);

            var serializedEvent = JsonConvert.SerializeObject(@event);
            var serializedMetadata = JsonConvert.SerializeObject(metadata);

            return new SerializedEvent(aggregateId, 1, serializedEvent, serializedMetadata, new Metadata(metadata));
        }
    }
}