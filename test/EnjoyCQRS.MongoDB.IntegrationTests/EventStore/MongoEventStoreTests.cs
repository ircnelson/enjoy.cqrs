using System.Threading.Tasks;
using EnjoyCQRS.EventStore.MongoDB;
using EnjoyCQRS.IntegrationTests.Shared.TestSuit;
using FluentAssertions;
using MongoDB.Embedded;
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
        public async Task Mongo_Events()
        {
            using (var embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);

                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                await eventStoreTestSuit.EventTestsAsync();
            }
        }

        [Fact]
        public async Task Mongo_Snapshot()
        {
            using (var embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);

                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                await eventStoreTestSuit.SnapshotTestsAsync();
            }
        }
    }
}