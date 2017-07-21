using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource.Storage;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.IntegrationTests
{
    [Trait("Integration", "WebApi")]
    public class FooWritableTests
    {
        [Fact]
        public async Task Should_create_foo()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();

            var server = TestServerFactory(eventStore);

            var response = await server.CreateRequest("/command/foo").PostAsync();

            var result = await response.Content.ReadAsStringAsync();

            var aggregateId = ExtractAggregateIdFromResponseContent(result);

            eventStore.Events.Count(e => e.AggregateId == aggregateId).Should().Be(1);
        }
        
        [Fact]
        public async Task Should_do_something()
        {
            var eventStore = new InMemoryEventStore();

            var server = TestServerFactory(eventStore);

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
            var eventStore = new InMemoryEventStore();

            var server = TestServerFactory(eventStore);

            var response = await server.CreateRequest("/command/foo/flood/4").PostAsync();

            eventStore.Events.Count.Should().Be(4);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        
        [Fact]
        public async Task Verify_custom_metadata()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();
            var eventsMetadataService = new EventsMetadataService();

            var server = TestServerFactory(eventStore, eventsMetadataService);

            var response = await server.CreateRequest("/command/foo").PostAsync();

            var eventsWithMetadata = eventsMetadataService.GetEvents().ToList();

            eventsWithMetadata.Count().Should().Be(1);
            var fakeUser = eventsWithMetadata[0].Metadata.GetValue(FakeUserMetadataProvider.MetadataKey, e => (User)e);

            fakeUser.Name.Should().Be("Xomano");
            fakeUser.UserCode.Should().Be(123);
        }

        private TestServer TestServerFactory(IEventStore eventStore, EventsMetadataService eventsMetadataService = null)
        {   
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(collection => {

                    collection.AddScoped<IEventsMetadataService>(e => eventsMetadataService);
                    collection.AddScoped(provider => eventStore);
                    collection.AddScoped<IMetadataProvider, FakeUserMetadataProvider>();
                });

            var testServer = new TestServer(builder);

            return testServer;
        }
        
        private Guid ExtractAggregateIdFromResponseContent(string content)
        {
            var match = Regex.Match(content, "{\"AggregateId\":\"(.*)\"}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var aggregateId = match.Groups[1].Value;

            return Guid.Parse(aggregateId);
        }
    }
}