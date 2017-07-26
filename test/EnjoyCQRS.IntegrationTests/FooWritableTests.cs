using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EnjoyCQRS.EventSource;
using Newtonsoft.Json;
using System.Collections.Generic;
using EnjoyCQRS.Stores;
using EnjoyCQRS.Core;
using EnjoyCQRS.Stores.InMemory;

namespace EnjoyCQRS.IntegrationTests
{
    [Trait("Integration", "WebApi")]
    public class FooWritableTests
    {
        [Fact]
        public async Task Should_create_foo()
        {
            // Arrange
            var stores = new InMemoryStores();

            var server = TestServerFactory(stores, stores);

            var response = await server.CreateRequest("/command/foo").PostAsync();

            var result = await response.Content.ReadAsStringAsync();

            var aggregateId = ExtractAggregateIdFromResponseContent(result);

            stores.Events.Count(e => e.AggregateId == aggregateId).Should().Be(1);
        }
        
        [Fact]
        public async Task Should_do_something()
        {
            var stores = new InMemoryStores();

            var server = TestServerFactory(stores, stores);

            var response = await server.CreateRequest("/command/foo").PostAsync();

            var result = await response.Content.ReadAsStringAsync();

            var aggregateId = ExtractAggregateIdFromResponseContent(result);

            response = await server.CreateRequest($"/command/foo/{aggregateId}/doSomething").PostAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            aggregateId.Should().NotBeEmpty();
        }
        
        [Fact]
        public async Task Should_emit_many_events()
        {
            var eventStore = new InMemoryStores();

            var server = TestServerFactory(eventStore, eventStore);

            var response = await server.CreateRequest("/command/foo/flood/4").PostAsync();

            eventStore.Events.Count.Should().Be(4);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        
        [Fact]
        public async Task Verify_custom_metadata()
        {
            // Arrange
            var eventStore = new InMemoryStores();
            var eventsMetadataService = new EventsMetadataService();

            var server = TestServerFactory(eventStore, eventStore, eventsMetadataService);

            var response = await server.CreateRequest("/command/foo").PostAsync();

            var eventsWithMetadata = eventsMetadataService.GetEvents().ToList();

            eventsWithMetadata.Count().Should().Be(1);
            var fakeUser = eventsWithMetadata[0].Metadata.GetValue(FakeUserMetadataProvider.MetadataKey, e => (User)e);

            fakeUser.Name.Should().Be("Xomano");
            fakeUser.UserCode.Should().Be(123);
        }

        private TestServer TestServerFactory(ITransaction transaction, ICompositeStores compositeStores, EventsMetadataService eventsMetadataService = null)
        {   
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services => {

                    services.AddScoped<IEventsMetadataService>(e => eventsMetadataService);
                    services.AddScoped<ITransaction>(provider => transaction);
                    services.AddScoped<IEventStore>(provider => compositeStores.EventStore);
                    services.AddScoped<ISnapshotStore>(provider => compositeStores.SnapshotStore);
                    services.AddScoped<IMetadataProvider, FakeUserMetadataProvider>();
                });

            var testServer = new TestServer(builder);

            return testServer;
        }
        
        private Guid ExtractAggregateIdFromResponseContent(string content)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);

            var aggregateId = Guid.Parse(dict["aggregateId"].ToString());

            return aggregateId;
        }
    }
}