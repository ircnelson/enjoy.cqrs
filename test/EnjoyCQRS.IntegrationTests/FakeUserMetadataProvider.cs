using EnjoyCQRS.EventSource;
using System.Collections.Generic;
using EnjoyCQRS.Events;
using EnjoyCQRS.Collections;

namespace EnjoyCQRS.IntegrationTests
{
    public class FakeUserMetadataProvider : IMetadataProvider
    {
        public const string MetadataKey = "user";

        public IEnumerable<KeyValuePair<string, object>> Provide<TAggregate>(TAggregate aggregate, IDomainEvent @event, IMetadataCollection metadata) where TAggregate : IAggregate
        {
            var user = new User
            {
                UserCode = 123,
                Name = "Xomano"
            };

            yield return new KeyValuePair<string, object>(MetadataKey, user);
        }
    }

    public class User
    {
        public int UserCode { get; set; }
        public string Name { get; set; }
    }
}
