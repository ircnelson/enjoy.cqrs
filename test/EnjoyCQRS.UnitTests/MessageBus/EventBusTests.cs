using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Bus.Direct;
using EnjoyCQRS.Events;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.MessageBus
{
    public class EventBusTests
    {
        [Fact]
        public void When_a_single_event_is_published_to_the_bus_containing_a_single_EventHandler()
        {
            var testEvent = new TestEvent(Guid.NewGuid());
            var handler = new FirstTestEventHandler();
            DefaultRouterMessages routerMessages = new DefaultRouterMessages();
            routerMessages.Register<TestEvent>(x => handler.Execute(x));

            DirectMessageBus directMessageBus = new DirectMessageBus(routerMessages);

            directMessageBus.Publish<IDomainEvent>(new[] { testEvent });
            directMessageBus.Commit();

            handler.Ids.First().Should().Be(testEvent.AggregateId);
        }

        [Fact]
        public void When_a_single_event_is_published_to_the_bus_containing_multiple_EventHandlers()
        {
            var testEvent = new TestEvent(Guid.NewGuid());
            var handler1 = new FirstTestEventHandler();
            var handler2 = new SecondTestEventHandler();

            DefaultRouterMessages routerMessages = new DefaultRouterMessages();
            routerMessages.Register<TestEvent>(x => handler1.Execute(x));
            routerMessages.Register<TestEvent>(x => handler2.Execute(x));

            DirectMessageBus directMessageBus = new DirectMessageBus(routerMessages);

            directMessageBus.Publish<IDomainEvent>(new[] { testEvent });
            directMessageBus.Commit();

            handler1.Ids.First().Should().Be(testEvent.AggregateId);
            handler2.Ids.First().Should().Be(testEvent.AggregateId);
        }

        public class FirstTestEventHandler : IEventHandler<TestEvent>
        {
            public List<Guid> Ids { get; } = new List<Guid>();

            public void Execute(TestEvent @event)
            {
                Ids.Add(@event.AggregateId);
            }
        }

        public class SecondTestEventHandler : IEventHandler<TestEvent>
        {
            public List<Guid> Ids { get; } = new List<Guid>();

            public void Execute(TestEvent @event)
            {
                Ids.Add(@event.AggregateId);
            }
        }

        public class TestEvent : DomainEvent
        {
            public TestEvent(Guid aggregateId) : base(aggregateId)
            {
            }
        }
    }
}