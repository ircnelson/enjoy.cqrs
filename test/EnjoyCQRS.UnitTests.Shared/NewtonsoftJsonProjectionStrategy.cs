using EnjoyCQRS.Projections;
using Newtonsoft.Json;
using System.IO;

namespace EnjoyCQRS.UnitTests.Shared
{
    public class NewtonsoftJsonProjectionStrategy : IProjectionStrategy
    {
        public TEntity Deserialize<TEntity>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<TEntity>(reader.ReadToEnd(), JsonSettings.Default);
            }
        }

        public string GetEntityBucket<TEntity>()
        {
            return typeof(TEntity).FullName.ToLowerInvariant();
        }

        public string GetEntityLocation<TEntity>(object key)
        {
            return $"{typeof(TEntity).FullName}+{key}".ToLowerInvariant();
        }

        public void Serialize<TEntity>(TEntity entity, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                var serialize = JsonConvert.SerializeObject(entity, JsonSettings.Default);

                writer.Write(serialize);
            }
        }
    }

    public static class JsonSettings
    {
        public static JsonSerializerSettings Default = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };
    }
}
