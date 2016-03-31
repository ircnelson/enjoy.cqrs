using System;
using System.Linq;
using FluentAssertions;
using MyCQRS.Events;
using MyCQRS.EventStore;
using MyCQRS.TestFramework;
using Xunit;

namespace MyCQRS.UnitTests.Domain
{
    public class Registered_event_in_aggregate : AggregateRootTestFixture<TestAggregateRoot>
    {
        protected override void When()
        {
            AggregateRoot.DoSomething("Walter White");
        }

        [Fact]
        public void Then_some_event_should_be_published()
        {
            PublishedEvents.Last().Should().BeAssignableTo<SomeEvent>();
        }
    }

    public class Not_registered_event_in_aggregate : AggregateRootTestFixture<TestAggregateRoot>
    {
        protected override void When()
        {
            AggregateRoot.DoSomethingWithoutEvent();
        }

        [Fact]
        public void Then_throws_an_exception()
        {
            CaughtException.Should().BeAssignableTo<ThereWasNoExceptionButOneWasExpectedException>();
        }
    }

    public class TestAggregateRoot : Aggregate
    {
        public string Name { get; private set; }

        public TestAggregateRoot()
        {
            On<SomeEvent>(x =>
            {
                Name = x.Name;
            });
        }
        
        public void DoSomething(string name)
        {
            Raise(new SomeEvent(name));
        }

        public void DoSomethingWithoutEvent()
        {
            Raise(new NotRegisteredEvent());
        }
    }

    public class SomeEvent : DomainEvent
    {
        public string Name { get; }

        public SomeEvent(string name)
        {
            Name = name;
        }
    }

    public class NotRegisteredEvent : DomainEvent
    {
    }
}