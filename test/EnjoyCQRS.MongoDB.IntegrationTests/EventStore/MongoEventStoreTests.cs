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
        public const string CategoryName = "Integration";
        public const string CategoryValue = "MongoDB";

        public const string DatabaseName = "test";

        [Trait(CategoryName, CategoryValue)]
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

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Test_events()
        {
            using (var embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);

                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                await eventStoreTestSuit.EventTestsAsync();
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Test_snapshot()
        {
            using (var embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);

                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                await eventStoreTestSuit.SnapshotTestsAsync();
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task When_any_exception_be_thrown()
        {
            using (var embeddedMongoDbServer = new EmbeddedMongoDbServer())
            {
                var eventStore = new MongoEventStore(embeddedMongoDbServer.Client, DatabaseName);
                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                await eventStoreTestSuit.DoSomeProblemAsync();
            }
        }
    }
}