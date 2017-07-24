using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using System;
using FluentAssertions;
using System.Collections.Concurrent;
using EnjoyCQRS.Projections;
using EnjoyCQRS.Projections.InMemory;
using EnjoyCQRS.UnitTests.Shared;
using Scrutor;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate.Projections;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EnjoyCQRS.IntegrationTests
{
    [Trait("Integration", "WebApi")]
    public class UserTests
    {
        private ProjectorMethodMapper _projectorMethodMapper = new ProjectorMethodMapper();
        private ConcurrentDictionary<string, byte[]> _projectionStore = new ConcurrentDictionary<string, byte[]>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>> _projections = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>>();

        [Fact]
        public async Task Should_create_user()
        {
            // Arrange
            var eventStore = new InMemoryEventStore();

            var server = TestServerFactory(eventStore);

            var response = await server.CreateRequest("/command/user").PostAsync();

            var result = await response.Content.ReadAsStringAsync();

            var aggregateId = ExtractAggregateIdFromResponseContent(result);

            eventStore.Events.Count(e => e.AggregateId == aggregateId).Should().Be(1);

            _projections.Count().Should().Be(2);

        }

        private TestServer TestServerFactory(IEventStore eventStore)
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services => {

                    services.AddScoped<IEventsMetadataService, EventsMetadataService>();
                    services.AddScoped(provider => eventStore);
                    
                    services.AddSingleton(_projectionStore);
                    services.AddTransient<IProjectionStrategy, NewtonsoftJsonProjectionStrategy>();
                    services.AddTransient<IProjectionStore>(c => new MemoryProjectionStore(c.GetRequiredService<IProjectionStrategy>(), _projections));
                    services.AddTransient(typeof(IProjectionWriter<,>), typeof(MemoryProjectionReaderWriter<,>));
                    services.AddTransient(typeof(IProjectionReader<,>), typeof(MemoryProjectionReaderWriter<,>));

                    services.Scan(e =>
                        e.FromAssemblyOf<FooAssembler>()
                            .AddClasses(c => c.AssignableTo<IProjector>())
                            .AsImplementedInterfaces());

                    services.AddSingleton(c => _projectorMethodMapper);
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
