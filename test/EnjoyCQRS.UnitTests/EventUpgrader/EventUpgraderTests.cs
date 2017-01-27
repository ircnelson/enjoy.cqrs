using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus.InProcess;
using EnjoyCQRS.MetadataProviders;
using EnjoyCQRS.UnitTests.Shared;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.FooAggregate;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.EventUpgrader
{
    public class EventUpgraderTests
    {
        [Fact]
        public void Should_convert_an_event_in_another_removing_property()
        {
            var v1 = new EventWithCounter(Guid.NewGuid(), "v1", 1);

            EventWithCounterUpdate eventV1Upgrade = new EventWithCounterUpdate();
            var events = eventV1Upgrade.Update(v1);

            var v2 = (EventWithoutCounter) events.First();

            v2.AggregateId.Should().Be(v1.AggregateId);
            v2.Something.Should().Be(v1.Something);
        }

        [Fact]
        public void Should_convert_an_event_in_another_adding_property()
        {
            var v1 = new EventWithoutCounter(Guid.NewGuid(), "v1");

            EventWithoutCounterUpdate eventV1Upgrade = new EventWithoutCounterUpdate();
            var events = eventV1Upgrade.Update(v1);

            var v2 = (EventWithCounter) events.First();

            v2.AggregateId.Should().Be(v1.AggregateId);
            v2.Something.Should().Be(v1.Something);
            v2.Counter.Should().Be(1);
        }

        [Fact]
        public async Task Throw_exception_when_does_not_exists_Subscribe_and_Updater()
        {
            // Arrange

            var id = Guid.NewGuid();

            var events = new List<IDomainEvent>
            {
                new FooCreated(id),
                new NameChanged(id, "Bruce Wayne"),
                new NameChanged(id, "Peter Parker"),
                new NameChanged(id, "Jean Grey")
            };

            var session = await ArrangeSessionAsync<Foo>(id, arrangeEvents: events.ToArray());

            // Act

            Func<Task> func = async () =>
            {
                var aggregate = await session.GetByIdAsync<Foo>(id);
            };

            // Assert

            func.ShouldThrowExactly<HandleNotFound>();
        }

        [Fact]
        public async Task Should_return_events_without_Updater()
        {
            // Arrange
            
            var id = Guid.NewGuid();

            var events = new List<IDomainEvent>
            {
                new FooCreated(id),
                new FullNameChanged(id, "Bruce", "Wayne"),
                new FullNameChanged(id, "Peter", "Parker"),
                new FullNameChanged(id, "Jean", "Grey")
            };

            var session = await ArrangeSessionAsync<Foo>(id, arrangeEvents: events.ToArray());

            // Act

            var aggregate = await session.GetByIdAsync<Foo>(id);

            // Assert

            aggregate.Id.Should().Be(id);
            aggregate.FirstName.Should().Be("Jean");
            aggregate.LastName.Should().Be("Grey");
            aggregate.Version.Should().Be(4);
        }

        [Fact]
        public async Task Should_convert_update_events()
        {
            // Arrange

            var id = Guid.NewGuid();

            var events = new List<IDomainEvent>
            {
                new FooCreated(id),
                new NameChanged(id, "Bruce Wayne"),
                new NameChanged(id, "Peter Parker"),
                new NameChanged(id, "Jean Grey")
            };

            var eventUpdaters = new Dictionary<Type, object>
            {
                { typeof(NameChanged), new NameChangedEventUpdate() }
            };

            var eventsUpdated = new List<IDomainEvent>();
            
            var mockEventUpdateManager = new Mock<IEventUpdateManager>();
            mockEventUpdateManager.Setup(e => e.Update(It.IsAny<IEnumerable<IDomainEvent>>()))
                .Callback<IEnumerable<IDomainEvent>>(enumerable =>
                {
                    foreach (var @event in enumerable)
                    {
                        var eventType = @event.GetType();

                        if (!eventUpdaters.ContainsKey(eventType))
                        {
                            eventsUpdated.Add(@event);

                            continue;
                        }

                        var eventUpdate = eventUpdaters[eventType];

                        var methodInfo = eventUpdate.GetType().GetMethod(nameof(IEventUpdate<object>.Update), BindingFlags.Instance | BindingFlags.Public);
                        var resultMethod = (IEnumerable<IDomainEvent>)methodInfo.Invoke(eventUpdate, new object[] { @event });

                        eventsUpdated.AddRange(resultMethod);
                    }
                }).Returns(eventsUpdated);
            
            var session = await ArrangeSessionAsync<Foo>(id, eventUpdateManager: mockEventUpdateManager.Object, arrangeEvents: events.ToArray());

            // Act
            
            var aggregate = await session.GetByIdAsync<Foo>(id);
            
            // Assert

            aggregate.Id.Should().Be(id);
            aggregate.FirstName.Should().Be("Jean");
            aggregate.LastName.Should().Be("Grey");
        }

        private async Task<Session> ArrangeSessionAsync<TAggregate>(Guid aggregateId, IEventUpdateManager eventUpdateManager = null, params IDomainEvent[] arrangeEvents)
            where TAggregate : Aggregate, new()
        {
            var metadataProviders = new IMetadataProvider[]
            {
                new AggregateTypeMetadataProvider(),
                new EventTypeMetadataProvider(),
                new CorrelationIdMetadataProvider()
            };

            var loggerFactory = new NoopLoggerFactory();
            var eventPublisher = new EventPublisher(new StubEventRouter());

            var eventStore = new InMemoryEventStore();
            var eventSerializer = new EventSerializer(new JsonTextSerializer());
            var snapshotSerializer = new SnapshotSerializer(new JsonTextSerializer());
            var projectionSerializer = new ProjectionSerializer();

            var session = new Session(loggerFactory, eventStore, eventPublisher, eventSerializer, snapshotSerializer, projectionSerializer, eventUpdateManager: eventUpdateManager);
            
            var aggregate = (TAggregate) Activator.CreateInstance(typeof(TAggregate), args: aggregateId);
            
            aggregate.UpdateVersion(arrangeEvents.Length - 1);

            var serializedEvents = arrangeEvents.Select((e, index) =>
            {
                index++;

                var metadatas =
                    metadataProviders.SelectMany(md => md.Provide(aggregate, e, EventSource.Metadata.Empty)).Concat(new[]
                    {
                        new KeyValuePair<string, object>(MetadataKeys.EventId, Guid.NewGuid()),
                        new KeyValuePair<string, object>(MetadataKeys.EventVersion, (aggregate.Version + index))
                    });
                return eventSerializer.Serialize(aggregate, e, new EventSource.Metadata(metadatas));
            });

            eventStore.BeginTransaction();

            await eventStore.SaveAsync(serializedEvents);
            await eventStore.CommitAsync();

            return session;
        }
    }

    public class NameChanged : DomainEvent
    {
        public string Name { get; }

        public NameChanged(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }


    public class NameChangedEventUpdate : IEventUpdate<NameChanged>
    {
        public IEnumerable<IDomainEvent> Update(NameChanged oldEvent)
        {
            var fname = oldEvent.Name.Split(' ')[0];
            var lname = oldEvent.Name.Split(' ')[1];

            yield return new FullNameChanged(oldEvent.AggregateId, fname, lname);
        }
    }

    public class EventWithCounterUpdate : IEventUpdate<EventWithCounter>
    {
        public IEnumerable<IDomainEvent> Update(EventWithCounter oldEvent)
        {
            yield return new EventWithoutCounter(oldEvent.AggregateId, oldEvent.Something);
        }
    }

    public class EventWithoutCounterUpdate : IEventUpdate<EventWithoutCounter>
    {
        public IEnumerable<IDomainEvent> Update(EventWithoutCounter oldEvent)
        {
            yield return new EventWithCounter(oldEvent.AggregateId, oldEvent.Something, 1);
        }
    }

    public class EventWithCounter : DomainEvent
    {
        public string Something { get; }

        public int Counter { get; }

        public EventWithCounter(Guid aggregateId, string something, int counter) : base(aggregateId)
        {
            Something = something;
            Counter = counter;
        }
    }

    public class EventWithoutCounter : DomainEvent
    {
        public string Something { get; }
        public EventWithoutCounter(Guid aggregateId, string something) : base(aggregateId)
        {
            Something = something;
        }
    }
}
