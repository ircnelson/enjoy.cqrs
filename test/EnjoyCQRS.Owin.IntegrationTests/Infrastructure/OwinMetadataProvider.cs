using System.Collections.Generic;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using Microsoft.Owin;

namespace EnjoyCQRS.Owin.IntegrationTests.Infrastructure
{
    public class OwinMetadataProvider : IMetadataProvider
    {
        private readonly IOwinContext _owinContext;

        public OwinMetadataProvider(IOwinContext owinContext)
        {
            _owinContext = owinContext;
        }

        public IEnumerable<KeyValuePair<string, string>> Provide<TAggregate>(TAggregate aggregate, IDomainEvent @event, IMetadata metadata) where TAggregate : IAggregate
        {
            yield return new KeyValuePair<string, string>("remoteIpAddress", _owinContext.Request.RemoteIpAddress);
        }
    }
}