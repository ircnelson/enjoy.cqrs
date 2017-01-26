using EnjoyCQRS.EventSource.Projections;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using GreenPipes.Internals.Reflection;
using MongoDB.Driver.Linq;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class MongoProjectionRepository : IProjectionRepository, IDisposable
    {
        public MongoClient Client { get; }
        public string Database { get; }
        public MongoEventStoreSetttings Setttings { get; }

        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Local
        };

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
            var builderFilter = Builders<MongoProjection>.Filter;
            var filter = builderFilter.Eq(x => x.Category, category)
                         & builderFilter.Eq(x => x.ProjectionId, id);

            var projection = await QuerySingleResult(projectionType, filter);

            return projection;
        }

        public async Task<object> GetAsync(Type projectionType, string name)
        {
            var builderFilter = Builders<MongoProjection>.Filter;
            var filter = builderFilter.Eq(x => x.Id, name);

            var projection = await QuerySingleResult(projectionType, filter);

            return projection;
        }

        public async Task<TProjection> GetAsync<TProjection>(string name)
        {
            return (TProjection)(await GetAsync(typeof(TProjection), name));
        }
        
        public async Task<TProjection> GetAsync<TProjection>(string category, Guid id)
        {
            return (TProjection)(await GetAsync(typeof(TProjection), category, id));
        }

        private async Task<object> QuerySingleResult(Type projectionType, FilterDefinition<MongoProjection> filter)
        {
            if (projectionType.GetTypeInfo().IsInterface)
            {
                var builder = new DynamicImplementationBuilder();

                projectionType = builder.GetImplementationType(projectionType);
            }
            
            var db = Client.GetDatabase(Database);
            var collection = db.GetCollection<MongoProjection>(Setttings.ProjectionsCollectionName);
            
            var record = await collection
                .Find(filter)
                .Limit(1)
                .FirstAsync();
            
            var json = JsonConvert.SerializeObject(record.Projection, JsonSerializerSettings);

            var projection = JsonConvert.DeserializeObject(json, projectionType, JsonSerializerSettings);
            
            return projection;
        }

        public void Dispose()
        {
        }
    }
}
