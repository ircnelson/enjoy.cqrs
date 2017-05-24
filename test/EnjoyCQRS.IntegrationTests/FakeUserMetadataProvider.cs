using EnjoyCQRS.EventSource;
using System.Collections.Generic;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests
{
    public class FakeUserMetadataProvider : IMetadataProvider
    {
        public const string MetadataKey = "user";

        public IEnumerable<KeyValuePair<string, object>> Provide<TAggregate>(TAggregate aggregate, IDomainEvent @event, IMetadata metadata) where TAggregate : IAggregate
        {
            var user = new
            {
                Name = "Xomano",
                UserCode = 123
            };

            yield return new KeyValuePair<string, object>(MetadataKey, user);
        }
    }
}
