using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.EventStore.MongoDB;
using EnjoyCQRS.UnitTests.Shared.TestSuit;
using FluentAssertions;
using MongoDB.Driver;
using Xunit;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate.Projections;

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
            Action action = () => new MongoEventStore(mongoClient, database, setttings);

            action.ShouldThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public async Task Test_events()
        {
            using (var eventStore = new MongoEventStore(_mongoClient, _fixture.DatabaseName))
            {
                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                var aggregate = await eventStoreTestSuit.EventTestsAsync();

                using (var projectionRepository = new MongoProjectionRepository(_mongoClient, _fixture.DatabaseName))
                {
                    var projection = await projectionRepository.GetAsync<BarProjection>(nameof(BarProjection), aggregate.Id);

                    projection.Id.Should().Be(aggregate.Id);
                    projection.LastText.Should().Be(aggregate.LastText);
                    projection.UpdatedAt.ToString("G").Should().Be(aggregate.UpdatedAt.ToString("G"));
                    projection.Messages.Count.Should().Be(aggregate.Messages.Count);
                }

                using (var projectionRepository = new MongoProjectionRepository<BarProjection>(_mongoClient, _fixture.DatabaseName))
                {
                    var projections = await projectionRepository.FindAsync(e => e.Id == aggregate.Id);

                    projections.Count().Should().BeGreaterOrEqualTo(1);
                }
            }
        }
        
        [Fact]
        public async Task Test_snapshot()
        {
            using (var eventStore = new MongoEventStore(_mongoClient, _fixture.DatabaseName))
            {
                var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

                await eventStoreTestSuit.SnapshotTestsAsync();
            }
        }
        
        [Fact]
        public async Task When_any_exception_be_thrown()
        {
            using (var eventStore = new MongoEventStore(_mongoClient, _fixture.DatabaseName))
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
    }
}