using EnjoyCQRS.EventSource.Projections;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class MongoProjectionRepository : IProjectionRepository, IDisposable
    {
        public MongoClient Client { get; }
        public string Database { get; }
        public MongoEventStoreSetttings Setttings { get; }
                
        public MongoProjectionRepository(MongoClient client, string database) : this(client, database, new MongoEventStoreSetttings())
        {
        }

        public MongoProjectionRepository(MongoClient client, string database, MongoEventStoreSetttings setttings)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (setttings == null) throw new ArgumentNullException(nameof(setttings));
           
            setttings.Validate();

            Database = database;
            Setttings = setttings;
            Client = client;
        }

        public async Task<object> GetAsync(Type projectionType, string category, Guid id)
        {
            var builderFilter = Builders<BsonDocument>.Filter;
            var filter = builderFilter.Eq(x => x["_id"], id) & builderFilter.Eq(x => x["_t"], category);

            var projection = await QuerySingleResult(projectionType, filter);

            return projection;
        }

        public async Task<object> GetAsync(Type projectionType, Guid id)
        {
            var builderFilter = Builders<BsonDocument>.Filter;
            var filter = builderFilter.Eq(x => x["_id"], id) & builderFilter.Eq(x => x["_t"], projectionType.Name);

            var projection = await QuerySingleResult(projectionType, filter);

            return projection;
        }

        public async Task<TProjection> GetAsync<TProjection>(Guid id)
        {
            return (TProjection)(await GetAsync(typeof(TProjection), id));
        }
        
        public async Task<TProjection> GetAsync<TProjection>(string category, Guid id)
        {
            return (TProjection)(await GetAsync(typeof(TProjection), category, id));
        }

        private async Task<object> QuerySingleResult(Type projectionType, FilterDefinition<BsonDocument> filter)
        {
            var db = Client.GetDatabase(Database);
            var collection = db.GetCollection<BsonDocument>(Setttings.ProjectionsCollectionName);
            
            var record = await collection
                .Find(filter)
                .Limit(1)
                .FirstAsync();

            var projection = BsonSerializer.Deserialize(record, projectionType);
            
            return projection;
        }
        
        public void Dispose()
        {
        }

    }
}
