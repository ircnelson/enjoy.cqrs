using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.EventStore.MongoDB;
using EnjoyCQRS.UnitTests.Shared.TestSuit;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;

namespace EnjoyCQRS.MongoDB.IntegrationTests.EventStore
{
    public class MongoEventStoreTests : IDisposable
    {
        public const string CategoryName = "Integration";
        public const string CategoryValue = "MongoDB";
        public const string DatabaseName = "enjoycqrs";

        private readonly MongoClient _mongoClient;

        public MongoEventStoreTests()
        {
            var mongoHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
            _mongoClient = new MongoClient($"mongodb://{mongoHost}");

            _mongoClient.DropDatabase(DatabaseName);
        }

        [Trait(CategoryName, CategoryValue)]
        [Theory, MemberData(nameof(InvalidStates))]
        public void Should_validate_constructor_parameters(MongoClient mongoClient, string database, MongoEventStoreSetttings setttings)
        {
            Action action = () => new MongoEventStore(mongoClient, database, setttings);

            action.ShouldThrowExactly<ArgumentNullException>();
        }
        

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_use_default_settings()
        {
            var defaultSettings = new MongoEventStoreSetttings();

            using (var eventStore = new MongoEventStore(_mongoClient, DatabaseName))
            {
                eventStore.Setttings.EventsCollectionName.Should().Be(defaultSettings.EventsCollectionName);
                eventStore.Setttings.SnapshotsCollectionName.Should().Be(defaultSettings.SnapshotsCollectionName);
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Theory]
        [InlineData("events", null)]
        [InlineData(null, "snapshots")]
        public void Should_validate_settings(string eventCollectionName, string snapshotCollectionName)
        {
            var defaultSettings = new MongoEventStoreSetttings
            {
                EventsCollectionName = eventCollectionName,
                SnapshotsCollectionName = snapshotCollectionName
            };
            
            Action action = () => new MongoEventStore(new MongoClient(), DatabaseName, defaultSettings);

            action.ShouldThrowExactly<ArgumentNullException>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_use_custom_settings()
        {
            var customSettings = new MongoEventStoreSetttings
            {
                EventsCollectionName = "MyEvents",
                SnapshotsCollectionName = "MySnapshots"
            };

            using (var eventStore = new MongoEventStore(_mongoClient, DatabaseName, customSettings))
            {
                eventStore.Setttings.EventsCollectionName.Should().Be(customSettings.EventsCollectionName);
                eventStore.Setttings.SnapshotsCollectionName.Should().Be(customSettings.SnapshotsCollectionName);
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_create_database()
        {
            using (var eventStore = new MongoEventStore(_mongoClient, DatabaseName))
            {
                var database = eventStore.Client.GetDatabase(DatabaseName);

                database.Should().NotBeNull();

                eventStore.Database.Should().Be(DatabaseName);
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Test_events()
        {
            using (var eventStore = new MongoEventStore(_mongoClient, DatabaseName))
            {
                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);
                await eventStoreTestSuit.EventTestsAsync();
            }
        }
        
        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Test_snapshot()
        {
            using (var eventStore = new MongoEventStore(_mongoClient, DatabaseName))
            {
                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                await eventStoreTestSuit.SnapshotTestsAsync();
            }
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task When_any_exception_be_thrown()
        {
            using (var eventStore = new MongoEventStore(_mongoClient, DatabaseName))
            {
                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                await eventStoreTestSuit.DoSomeProblemAsync();
            }
        }

        public static IEnumerable<object[]> InvalidStates => new[]
        {
            new object[] { null, "dbname", new MongoEventStoreSetttings() },
            new object[] { new MongoClient(), null, new MongoEventStoreSetttings() },
            new object[] { new MongoClient(), "dbname", null }
        };

        public void Dispose()
        {
            _mongoClient.DropDatabase(DatabaseName);
        }
    }
}