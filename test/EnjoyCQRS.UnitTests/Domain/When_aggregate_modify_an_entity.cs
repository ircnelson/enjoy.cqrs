using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;
using FluentAssertions;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class When_aggregate_modify_an_entity : AggregateTestFixture<StubSnapshotAggregate>
    {
        private ChildCreatedEvent Entity2 = new ChildCreatedEvent(Guid.NewGuid(), "Child 2");

        protected override IEnumerable<IDomainEvent> Given()
        {
            yield return PrepareDomainEvent.Set(new StubAggregateCreatedEvent(Guid.NewGuid(), "Mother")).ToVersion(1);
            yield return PrepareDomainEvent.Set(new ChildCreatedEvent(Guid.NewGuid(), "Child 1")).ToVersion(1);
            yield return PrepareDomainEvent.Set(Entity2).ToVersion(1);
        }

        protected override void When()
        {
            AggregateRoot.DisableEntity(Entity2.Id);
        }

        [Then]
        public void Aggregate_should_have_2_items()
        {
            AggregateRoot.Entities.Should().HaveCount(2);
        }

        [Then]
        public void Should_be_published_an_event_that_entity_was_disabled()
        {
            PublishedEvents.Last().Should().BeOfType<ChildDisabledEvent>();
        }

        [Then]
        public void Should_verify_last_event_properties()
        {
            var childDisabledEvent = PublishedEvents.Last().As<ChildDisabledEvent>();

            childDisabledEvent.EntityId.Should().Be(Entity2.Id);
        }

        [Then]
        public void Entity2_should_be_disabled()
        {
            AggregateRoot.Entities[1].Enabled.Should().BeFalse();
        }
    }
}