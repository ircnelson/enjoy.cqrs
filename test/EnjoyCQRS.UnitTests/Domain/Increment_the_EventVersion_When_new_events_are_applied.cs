using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;
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
            yield return PrepareDomainEvent.Set(new NameChangedEvent(Guid.NewGuid(), "Walter White")).ToVersion(1);
        }

        protected override void When()
        {
            AggregateRoot.ChangeName("DoSomething");
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Then_the_Version_should_be_1()
        {
            AggregateRoot.Version.Should().Be(1);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Then_the_EventVersion_should_be_2()
        {
            AggregateRoot.EventVersion.Should().Be(2);
        }
    }
}