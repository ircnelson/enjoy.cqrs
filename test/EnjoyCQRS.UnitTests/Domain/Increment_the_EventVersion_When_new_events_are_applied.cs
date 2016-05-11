using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Events;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class Increment_the_EventVersion_When_new_events_are_applied : AggregateTestFixture<StubAggregate>
    {
        public const string CategoryName = "Unit";
        public const string CategoryValue = "Aggregate";

        protected override IEnumerable<IDomainEvent> Given()
        {
            yield return PrepareDomainEvent.Set(new SomeEvent(Guid.NewGuid(), "Walter White")).ToVersion(1);
        }

        protected override void When()
        {
            AggregateRoot.DoSomething("DoSomething");
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Then_the_Version_should_be_1()
        {
            AggregateRoot.Version.Should().Be(1);
        }

        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Then_the_EventVersion_should_be_2()
        {
            AggregateRoot.EventVersion.Should().Be(2);
        }
    }
}