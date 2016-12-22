using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.MetadataProviders
{
    public class CorrelationIdMetadataProvider : IMetadataProvider
    {
        private Guid _correlationId;

        public CorrelationIdMetadataProvider()
        {
            _correlationId = Guid.NewGuid();
        }

        public IEnumerable<KeyValuePair<string, object>> Provide<TAggregate>(TAggregate aggregate, IDomainEvent @event, IMetadata metadata) where TAggregate : IAggregate
        {
            yield return new KeyValuePair<string, object>(MetadataKeys.CorrelationId, _correlationId);
        }
    }
}