using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;
using EnjoyCQRS.TestFramework;
using EnjoyCQRS.UnitTests.Domain.AggregateWithEntities;
using EnjoyCQRS.UnitTests.Domain.AggregateWithEntities.Events;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class When_do_something_on_aggregate : AggregateTestFixture<ComplexStubAggregate>
    {
        protected override IEnumerable<IDomainEvent> Given()
        {
            yield return PrepareDomainEvent.Set(new ComplexStubAggregateCreatedEvent(Guid.NewGuid(), "Mother")).ToVersion(1);
            yield return PrepareDomainEvent.Set(new ChildCreatedEvent(Guid.NewGuid(), "Child 1")).ToVersion(1);
            yield return PrepareDomainEvent.Set(new ChildCreatedEvent(Guid.NewGuid(), "Child 2")).ToVersion(1);
        }

        protected override void When()
        {
            var child = AggregateRoot.Entities[1].Id;

            AggregateRoot.DisableEntity(child);
        }

        [Fact]
        public void Should_be_published_an_event_that_child_was_disabled()
        {
            PublishedEvents.Last().Should().BeOfType<ChildDisabledEvent>();
        }
    }
}