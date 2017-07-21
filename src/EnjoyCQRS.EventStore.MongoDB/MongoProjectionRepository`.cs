using EnjoyCQRS.EventSource.Projections;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

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

        public async Task<TProjection> GetAsync(Guid id)
        {
            return (TProjection)await GetAsync(typeof(TProjection), id);
        }
        
        public Task<IEnumerable<TProjection>> FindAsync(Expression<Func<TProjection, bool>> expr)
        {
            var category = ExtractCategoryOfType<TProjection>();

            return FindAsync(category, expr);
        }

        public async Task<IEnumerable<TProjection>> FindAsync(string category, Expression<Func<TProjection, bool>> expr)
        {
            var db = Client.GetDatabase(Database);
            var collection = db.GetCollection<BsonDocument>(Setttings.ProjectionsCollectionName).AsQueryable();

            var query = collection.OfType<TProjection>().Where(expr);

            return await query.ToListAsync();
        }

        private string ExtractCategoryOfType<T>()
        {
            var type = typeof(T);

            var category = type.Name;

            if (char.IsUpper(type.Name[0]) && type.Name.StartsWith("I") && type.GetTypeInfo().IsInterface)
                category = typeof(TProjection).Name.Substring(1);

            return category;
        }
    }
}
