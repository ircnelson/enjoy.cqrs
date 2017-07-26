using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.EventStore.MongoDB;
using EnjoyCQRS.UnitTests.Shared.TestSuit;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;
using EnjoyCQRS.EventStore.MongoDB.Stores;

namespace EnjoyCQRS.MongoDB.IntegrationTests
{
    [Collection("MongoDB")]
    [Trait("Integration", "MongoDB")]
    public class MongoEventStoreTests
    {   
        private readonly MongoClient _mongoClient;
        private readonly DatabaseFixture _fixture;

        public MongoEventStoreTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _mongoClient = fixture.Client;
        }
                
        [Theory, MemberData(nameof(InvalidStates))]
        public void Should_validate_constructor_parameters(MongoClient mongoClient, string database, MongoEventStoreSetttings setttings)
        {
            Action action = () => new MongoStores(mongoClient, database, setttings);

            action.ShouldThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public async Task Test_events()
        {
            using (var store = new MongoStores(_mongoClient, _fixture.DatabaseName))
            {
                var eventStoreTestSuit = new EventStoreTestSuit(store, store);

                var aggregate = await eventStoreTestSuit.EventTestsAsync();

                aggregate.Should().NotBeNull();
            }
        }
        
        [Fact]
        public async Task Test_snapshot()
        {
            using (var stores = new MongoStores(_mongoClient, _fixture.DatabaseName))
            {
                var eventStoreTestSuit = new EventStoreTestSuit(stores, stores);

                await eventStoreTestSuit.SnapshotTestsAsync();
            }
        }
        
        [Fact]
        public async Task When_any_exception_be_thrown()
        {
            using (var stores = new MongoStores(_mongoClient, _fixture.DatabaseName))
            {
                var eventStoreTestSuit = new EventStoreTestSuit(stores, stores);

                await eventStoreTestSuit.DoSomeProblemAsync();
            }
        }

        public static IEnumerable<object[]> InvalidStates => new[]
        {
            new object[] { null, "dbname", new MongoEventStoreSetttings() },
            new object[] { new MongoClient(), null, new MongoEventStoreSetttings() },
            new object[] { new MongoClient(), "dbname", null }
        };
    }
}