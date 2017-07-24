using EnjoyCQRS.Events;
using EnjoyCQRS.Projections;
using EnjoyCQRS.Projections.InMemory;
using EnjoyCQRS.UnitTests.Shared;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate.Projections;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EnjoyCQRS.UnitTests.Projections
{
    public static class JsonSettings
    {
        public static JsonSerializerSettings Default = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };
    }

    [Trait("Unit", "Projection")]
    public class ProjectionTests
    {
        [Fact]
        public async Task Should_project_one_view()
        {
            // Arrange

            var userId = Guid.NewGuid();

            var source = new IDomainEvent[]
            {
                new UserCreated(userId, DateTime.Now, "Bryan", "Cranston", new DateTime(1956, 3, 7)),
                new UserFirstNameChanged(userId, "Walter"),
                new UserLastNameChanged(userId, "White"),
                new UserFirstNameChanged(userId, "Walt"),
                new UserLastNameChanged(userId, "Heisenberg")
            };

            var eventStreamReader = new StubEventStreamReader(source);
            
            var strategy = new NewtonsoftJsonProjectionStrategy();
            var store = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>>();
            var documentStore = new MemoryProjectionStore(strategy, store);

            var writer = documentStore.GetWriter<Guid, AllUserView>();
            var reader = documentStore.GetReader<Guid, AllUserView>();
            var allUserProjection = new AllUserProjections(writer);

            // Act
            
            var projectionProcessor = new ProjectionRebuilder(eventStreamReader, documentStore, new object[] { allUserProjection });

            await projectionProcessor.ProcessAsync().ConfigureAwait(false);
                        
            // Assert

            store.Count.Should().Be(1);

            reader.TryGet(userId, out AllUserView view1).Should().BeTrue();

            if (view1 != null)
            {
                view1.Id.Should().Be(userId);
                view1.BirthMonth.Should().Be(3);
                view1.BirthYear.Should().Be(1956);
                view1.Fullname.Should().Be("Heisenberg, Walt");
                view1.DeactivatedAt.Should().BeNull();
                view1.Lifetime.Should().BeNull();
            }
        }
        
        [Fact]
        public async Task Should_project_multiples_views()
        {
            // Arrange

            var activeUserId = Guid.NewGuid();
            var inactiveUserId = Guid.NewGuid();

            var source = new IDomainEvent[]
            {
                new UserCreated(activeUserId, DateTime.Now, "Bryan", "Cranston", new DateTime(1956, 3, 7)),
                new UserFirstNameChanged(activeUserId, "Walter"),
                new UserLastNameChanged(activeUserId, "White"),
                new UserFirstNameChanged(activeUserId, "Walt"),
                new UserLastNameChanged(activeUserId, "Heisenberg"),

                new UserCreated(inactiveUserId, DateTime.Now, "Aaron Paul", "Sturtevant", new DateTime(1979, 8, 27)),
                new UserLastNameChanged(inactiveUserId, "Bruce Pinkman"),
                new UserFirstNameChanged(inactiveUserId, "Jesse"),
                new UserDeactivated(inactiveUserId, new DateTime(2017, 7, 4))
            };

            var eventStreamReader = new StubEventStreamReader(source);

            var strategy = new NewtonsoftJsonProjectionStrategy();
            var store = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>>();
            var documentStore = new MemoryProjectionStore(strategy, store);

            var writer1 = documentStore.GetWriter<Guid, AllUserView>();
            var allUserProjection = new AllUserProjections(writer1);

            var writer2 = documentStore.GetWriter<Guid, ActiveUserView>();
            var onlyActiveUserProjection = new OnlyActiveUserProjections(writer2);

            // Act
            
            var projectionProcessor = new ProjectionRebuilder(eventStreamReader, documentStore, new object[] { allUserProjection, onlyActiveUserProjection });

            await projectionProcessor.ProcessAsync().ConfigureAwait(false);

            // Assert

            store.Count.Should().Be(2);

            var reader1 = documentStore.GetReader<Guid, AllUserView>();

            reader1.TryGet(activeUserId, out AllUserView view1).Should().BeTrue();

            if (view1 != null)
            {
                view1.Id.Should().Be(activeUserId);
                view1.BirthMonth.Should().Be(3);
                view1.BirthYear.Should().Be(1956);
                view1.Fullname.Should().Be("Heisenberg, Walt");
                view1.DeactivatedAt.Should().BeNull();
                view1.Lifetime.Should().BeNull();
            }

            reader1.TryGet(inactiveUserId, out view1).Should().BeTrue();

            if (view1 != null)
            {
                view1.Id.Should().Be(inactiveUserId);
                view1.BirthMonth.Should().Be(8);
                view1.BirthYear.Should().Be(1979);
                view1.Fullname.Should().Be("Bruce Pinkman, Jesse");
                view1.DeactivatedAt.Should().Be(new DateTime(2017, 7, 4));
                view1.Lifetime.Should().NotBeNull();
            }

            var reader2 = documentStore.GetReader<Guid, ActiveUserView>();

            reader2.TryGet(activeUserId, out ActiveUserView view2).Should().BeTrue();

            if (view2 != null)
            {
                view2.Id.Should().Be(activeUserId);
            }

            reader2.TryGet(inactiveUserId, out view2).Should().BeFalse();
        }
    }

    class StubEventStreamReader : EventStreamReader
    {
        private readonly Stream _source;

        public StubEventStreamReader(params IDomainEvent[] events)
        {
            _source = CreateStreamFromSource(events);
        }

        public override async Task ReadAsync(CancellationToken cancellationToken, OnDeserializeEventDelegate wireDelegate)
        {
            using (var sr = new StreamReader(_source, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var json = await sr.ReadLineAsync();

                    var @event = JsonConvert.DeserializeObject(json, JsonSettings.Default);

                    wireDelegate(@event, null);
                }
            }
        }

        private static Stream CreateStreamFromSource(IEnumerable<IDomainEvent> events)
        {
            var nl = Encoding.UTF8.GetBytes("\n");

            Stream stream;
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (var @event in events)
                {
                    var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, JsonSettings.Default));

                    ms.Write(json, 0, json.Length);
                    ms.Write(nl, 0, nl.Length);
                }

                stream = new MemoryStream(ms.ToArray(), false);
            }

            return stream;
        }
    }
}
