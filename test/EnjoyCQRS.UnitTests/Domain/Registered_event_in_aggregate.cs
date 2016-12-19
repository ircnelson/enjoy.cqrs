using System.Linq;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;
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
            AggregateRoot = StubAggregate.Create("Heinsenberg");
            AggregateRoot.ChangeName("Walter White");
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Then_some_event_should_be_published()
        {
            PublishedEvents.Last().Should().BeAssignableTo<NameChangedEvent>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Then_verify_name_property()
        {
            PublishedEvents.Last().As<NameChangedEvent>().Name.Should().Be(AggregateRoot.Name);
        }
    }
}