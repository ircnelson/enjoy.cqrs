using System;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using Xunit;
using EnjoyCQRS.EventStore.MongoDB;

namespace EnjoyCQRS.MongoDB.IntegrationTests
{
    public class DatabaseFixture : IDisposable
    {
        public readonly MongoEventStoreSetttings Settings = new MongoEventStoreSetttings();

        public string DatabaseName { get; private set; } = "enjoycqrs";

        public MongoClient Client { get; private set; }
        public IMongoDatabase Database { get; private set; }

        public DatabaseFixture()
        {
            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            };

            ConventionRegistry.Register("camelCase", pack, t => true);
            BsonSerializer.RegisterSerializer(typeof(DateTime), DateTimeSerializer.LocalInstance);

            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            var mongoHost = Environment.GetEnvironmentVariable("MONGODB_HOST");

            if (string.IsNullOrWhiteSpace(mongoHost))
            {
                mongoHost = "localhost";
            }
            
            Client = new MongoClient($"mongodb://{mongoHost}");

            Client.DropDatabase(DatabaseName);

            Database = Client.GetDatabase(DatabaseName);
            
            var keys = Builders<BsonDocument>.IndexKeys.Ascending(_ => _["_id"]).Ascending(_ => _["_t"]);

            foreach (var collection in new[] { Settings.ProjectionsCollectionName, Settings.TempProjectionsCollectionName })
            {
                Database.CreateCollection(collection, new CreateCollectionOptions
                {
                    AutoIndexId = false
                });

                Database.GetCollection<BsonDocument>(collection).Indexes.CreateOne(keys, new CreateIndexOptions
                {
                    Unique = true
                });
            }
        }

        public void Dispose()
        {
            Client.DropDatabase(DatabaseName);
        }
    }

    [CollectionDefinition("MongoDB")]
    public class MongoDatabaseCollection : ICollectionFixture<DatabaseFixture>
    {

    }
}