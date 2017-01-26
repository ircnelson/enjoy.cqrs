using EnjoyCQRS.EventSource.Projections;
using MongoDB.Driver;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace EnjoyCQRS.EventStore.MongoDB
{
    public class MongoProjectionRepository<TProjection> : MongoProjectionRepository, IProjectionRepository<TProjection>
    {
        public MongoProjectionRepository(MongoClient client, string database) : base(client, database)
        {
        }

        public MongoProjectionRepository(MongoClient client, string database, MongoEventStoreSetttings setttings) : base(client, database, setttings)
        {
        }

        public async Task<TProjection> GetAsync(string name)
        {
            return (TProjection)await GetAsync(typeof(TProjection), name);
        }

        public async Task<TProjection> GetAsync(Guid id)
        {
            var type = typeof(TProjection);

            string category = type.Name;

            if (char.IsUpper(type.Name[0]) && type.Name.StartsWith("I") && type.GetTypeInfo().IsInterface)
                category = typeof(TProjection).Name.Substring(1);

            return (TProjection)await GetAsync(typeof(TProjection), category, id);
        }
    }
}
