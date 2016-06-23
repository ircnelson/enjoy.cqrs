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
    public class FakeGameWritableTests
    {
        [Fact]
        public async Task Should_emit_many_events()
        {
            var eventStore = new InMemoryEventStore();

            var server = TestServerFactory(eventStore);

            var response = await server.CreateRequest("/command/fakeGame/flood/4").PostAsync();

            eventStore.Events.Count.Should().Be(4);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
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
