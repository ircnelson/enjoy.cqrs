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
    public class When_aggregate_modify_an_entity : AggregateTestFixture<StubSnapshotAggregate>
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Snapshot";
        
        private readonly ChildCreatedEvent _entity2 = new ChildCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Child 2");

        protected override IEnumerable<IDomainEvent> Given()
        {
            yield return new StubAggregateCreatedEvent(Guid.NewGuid(), "Mother");
            yield return new ChildCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Child 1");
            yield return _entity2;
        }

        protected override void When()
        {
            AggregateRoot.DisableEntity(_entity2.EntityId);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Aggregate_should_have_2_items()
        {
            AggregateRoot.Entities.Should().HaveCount(2);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_be_published_an_event_that_entity_was_disabled()
        {
            PublishedEvents.Last().Should().BeOfType<ChildDisabledEvent>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_verify_last_event_properties()
        {
            var childDisabledEvent = PublishedEvents.Last().As<ChildDisabledEvent>();

            childDisabledEvent.EntityId.Should().Be(_entity2.EntityId);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Entity2_should_be_disabled()
        {
            AggregateRoot.Entities[1].Enabled.Should().BeFalse();
        }
    }
}