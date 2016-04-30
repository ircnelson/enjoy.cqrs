using System.Linq;
using EnjoyCQRS.EventStore.Exceptions;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Events;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class Registered_event_in_aggregate : AggregateTestFixture<TestAggregateRoot>
    {
        protected override void When()
        {
            AggregateRoot = TestAggregateRoot.Create();
            AggregateRoot.DoSomething("Walter White");
        }

        [Fact]
        public void Then_some_event_should_be_published()
        {
            PublishedEvents.Last().Should().BeAssignableTo<SomeEvent>();
        }
    }

    public class Not_registered_event_in_aggregate : AggregateTestFixture<TestAggregateRoot>
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
}