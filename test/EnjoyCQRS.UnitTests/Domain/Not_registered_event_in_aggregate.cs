using EnjoyCQRS.EventSource.Exceptions;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class Not_registered_event_in_aggregate : AggregateTestFixture<StubAggregate>
    {
        public const string CategoryName = "Unit";
        public const string CategoryValue = "Aggregate";

        protected override void When()
        {
            AggregateRoot.DoSomethingWithoutEventSubscription();
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Then_throws_an_exception()
        {
            CaughtException.Should().BeAssignableTo<HandleNotFound>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Then_the_event_type_should_be_SomeEvent()
        {
            CaughtException.As<HandleNotFound>().EventType.Should().BeAssignableTo<NotRegisteredEvent>();
        }
    }
}