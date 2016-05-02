using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Events;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class Registered_event_in_aggregate : AggregateTestFixture<StubAggregate>
    {
        protected override void When()
        {
            AggregateRoot = StubAggregate.Create();
            AggregateRoot.DoSomething("Walter White");
        }

        [Fact]
        public void Then_some_event_should_be_published()
        {
            PublishedEvents.Last().Should().BeAssignableTo<SomeEvent>();
        }
    }

    public class Not_registered_event_in_aggregate : AggregateTestFixture<StubAggregate>
    {
        protected override void When()
        {
            AggregateRoot.DoSomethingWithoutEvent();
        }

        [Fact]
        public void Then_throws_an_exception()
        {
            CaughtException.Should().BeAssignableTo<HandleNotFound>();
        }

        [Fact]
        public void Then_the_event_type_should_be_SomeEvent()
        {
            CaughtException.As<HandleNotFound>().EventType.Should().BeAssignableTo<NotRegisteredEvent>();
        }
    }

    public class Increment_the_EventVersion_When_new_events_are_applied : AggregateTestFixture<StubAggregate>
    {
        protected override IEnumerable<IDomainEvent> Given()
        {
            yield return PrepareDomainEvent.Set(new SomeEvent(Guid.NewGuid(), "Walter White")).ToVersion(1);
        }

        protected override void When()
        {
            AggregateRoot.DoSomething("DoSomething");
        }

        [Fact]
        public void Then_the_Version_should_be_1()
        {
            AggregateRoot.Version.Should().Be(1);
        }

        [Fact]
        public void Then_the_EventVersion_should_be_2()
        {
            AggregateRoot.EventVersion.Should().Be(2);
        }
    }
}