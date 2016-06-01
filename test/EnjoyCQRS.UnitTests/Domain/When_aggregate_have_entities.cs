using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class When_aggregate_have_entities : AggregateTestFixture<StubSnapshotAggregate>
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Snapshot";

        private string newChildName = "New child";

        protected override IEnumerable<IDomainEvent> Given()
        {
            yield return PrepareDomainEvent.Set(new StubAggregateCreatedEvent(Guid.NewGuid(), "Mother")).ToVersion(1);
            yield return PrepareDomainEvent.Set(new ChildCreatedEvent(Guid.NewGuid(), "Child 1")).ToVersion(1);
            yield return PrepareDomainEvent.Set(new ChildCreatedEvent(Guid.NewGuid(), "Child 2")).ToVersion(1);
        }

        protected override void When()
        {
            AggregateRoot.AddEntity(newChildName);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Aggregate_should_have_3_items()
        {
            AggregateRoot.Entities.Should().HaveCount(3);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_be_published_an_event_that_entity_was_created()
        {
            PublishedEvents.Last().Should().BeOfType<ChildCreatedEvent>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_verify_last_event_properties()
        {
            var childCreatedEvent = PublishedEvents.Last().As<ChildCreatedEvent>();

            childCreatedEvent.Name.Should().Be(newChildName);
        }
    }
}