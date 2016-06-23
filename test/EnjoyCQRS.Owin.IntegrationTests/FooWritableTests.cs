using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Owin.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Owin.Testing;
using Xunit;

namespace EnjoyCQRS.Owin.IntegrationTests
{
    public class FooWritableTests
    {
        [Fact]
        public async Task Should_create_foo()
        {
            var eventStore = new InMemoryEventStore();

            var server = TestServerFactory(eventStore);

            await server.CreateRequest("/command/foo").PostAsync();

            eventStore.Events.Count.Should().Be(1);
        }

        [Fact]
        public async Task Should_do_something()
        {
            var eventStore = new InMemoryEventStore();

            var server = TestServerFactory(eventStore);

            var response = await server.CreateRequest("/command/foo").PostAsync();

            var result = await response.Content.ReadAsStringAsync();

            var match = Regex.Match(result, "{\"AggregateId\":\"(.*)\"}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var aggregateId = match.Groups[1].Value;

            response = await server.CreateRequest($"/command/foo/{aggregateId}/doSomething").PostAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            aggregateId.Should().NotBeNullOrWhiteSpace();
        }
        
        private TestServer TestServerFactory(IEventStore eventStore)
        {
            var startup = new Startup
            {
                EventStore = eventStore
            };

            var testServer = TestServer.Create(startup.Configuration);

            return testServer;
        }
    }
}
