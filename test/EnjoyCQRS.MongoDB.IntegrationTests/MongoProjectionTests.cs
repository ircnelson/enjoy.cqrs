using EnjoyCQRS.Projections;
using EnjoyCQRS.UnitTests.Shared.Projection;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Xunit;
using EnjoyCQRS.EventStore.MongoDB;
using EnjoyCQRS.UnitTests.Shared.Helpers;
using EnjoyCQRS.EventStore.MongoDB.Projection;

namespace EnjoyCQRS.MongoDB.IntegrationTests
{
    [Collection("MongoDB")]
    [Trait("Integration", "MongoDB")]
    public class MongoProjectionTests
    {
        private readonly DatabaseFixture _fixture;

        public MongoProjectionTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Should_project_multiples_views()
        {
            // Arrange

            var activeUser = new UserAggregate(Guid.NewGuid(), "Bryan", "Cranston", new DateTime(1956, 3, 7));
            activeUser.ChangeFirstName("Walter");
            activeUser.ChangeLastName("White");
            activeUser.ChangeFirstName("Walt");
            activeUser.ChangeLastName("Heisenberg");

            var inactiveUser = new UserAggregate(Guid.NewGuid(), "Aaron Paul", "Sturtevant", new DateTime(1979, 8, 27));
            inactiveUser.ChangeLastName("Bruce Pinkman");
            inactiveUser.ChangeFirstName("Jesse");
            inactiveUser.Deactivate();

            var eventStore = new MongoEventStore(_fixture.Database);
            var session = SessionHelper.Create(eventStore);

            await session.AddAsync(activeUser).ConfigureAwait(false);
            await session.AddAsync(inactiveUser).ConfigureAwait(false);

            await session.SaveChangesAsync().ConfigureAwait(false);

            var eventStreamReader = new MongoEventStreamReader(_fixture.Database);
            
            var strategy = new MongoProjectionStrategy();
            
            var documentStore = new MongoProjectionStore(strategy, _fixture.Database);

            var writer1 = documentStore.GetWriter<Guid, AllUserView>();
            var allUserProjection = new AllUserProjections(writer1);

            var writer2 = documentStore.GetWriter<Guid, ActiveUserView>();
            var onlyActiveUserProjection = new OnlyActiveUserProjections(writer2);

            // Act

            var projectionProcessor = new ProjectionRebuilder(eventStreamReader, documentStore, new object[] { allUserProjection, onlyActiveUserProjection });

            await projectionProcessor.ProcessAsync().ConfigureAwait(false);

            // Assert

            var tempCollection = _fixture.Database.GetCollection<BsonDocument>(_fixture.Settings.TempProjectionsCollectionName);
            var collection = _fixture.Database.GetCollection<BsonDocument>(_fixture.Settings.ProjectionsCollectionName);

            var filterBuilder = new FilterDefinitionBuilder<BsonDocument>();
            var filter = filterBuilder.In("_t", new[] { nameof(AllUserView), nameof(ActiveUserView) });

            tempCollection.Count(filter).Should().Be(3);
            collection.Count(filter).Should().Be(3);

            var reader1 = documentStore.GetReader<Guid, AllUserView>();

            reader1.TryGet(activeUser.Id, out AllUserView view1).Should().BeTrue();

            if (view1 != null)
            {
                view1.Id.Should().Be(activeUser.Id);
                view1.BirthMonth.Should().Be(3);
                view1.BirthYear.Should().Be(1956);
                view1.Fullname.Should().Be("Heisenberg, Walt");
                view1.DeactivatedAt.Should().BeNull();
                view1.Lifetime.Should().BeNull();
            }

            reader1.TryGet(inactiveUser.Id, out view1).Should().BeTrue();

            if (view1 != null)
            {
                view1.Id.Should().Be(inactiveUser.Id);
                view1.BirthMonth.Should().Be(8);
                view1.BirthYear.Should().Be(1979);
                view1.Fullname.Should().Be("Bruce Pinkman, Jesse");
                view1.DeactivatedAt.Should().NotBeNull();
                view1.Lifetime.Should().NotBeNull();
            }

            var reader2 = documentStore.GetReader<Guid, ActiveUserView>();

            reader2.TryGet(activeUser.Id, out ActiveUserView view2).Should().BeTrue();

            if (view2 != null)
            {
                view2.Id.Should().Be(activeUser.Id);
            }

            reader2.TryGet(inactiveUser.Id, out view2).Should().BeFalse();
        }
    }
}
