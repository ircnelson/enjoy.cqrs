using System.Linq;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Events;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class Registered_event_in_aggregate : AggregateTestFixture<StubAggregate>
    {
        public const string CategoryName = "Unit";
        public const string CategoryValue = "Aggregate";

        protected override void When()
        {
            AggregateRoot = StubAggregate.Create();
            AggregateRoot.DoSomething("Walter White");
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Then_some_event_should_be_published()
        {
            PublishedEvents.Last().Should().BeAssignableTo<SomeEvent>();
        }
    }
}