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
    public class When_aggregate_have_children : AggregateTestFixture<ComplexStubAggregate>
    {
        protected override IEnumerable<IDomainEvent> Given()
        {
            yield return PrepareDomainEvent.Set(new ComplexStubAggregateCreatedEvent(Guid.NewGuid(), "Mother")).ToVersion(1);
            yield return PrepareDomainEvent.Set(new ChildCreatedEvent(Guid.NewGuid(), "Child 1")).ToVersion(1);
            yield return PrepareDomainEvent.Set(new ChildCreatedEvent(Guid.NewGuid(), "Child 2")).ToVersion(1);
        }

        protected override void When()
        {
            AggregateRoot.AddEntity("New child");
        }

        [Fact]
        public void Aggregate_should_have_2_children()
        {
            AggregateRoot.Entities.Should().HaveCount(3);
        }

        [Fact]
        public void Should_be_published_an_event_that_child_was_created()
        {
            PublishedEvents.Last().Should().BeOfType<ChildCreatedEvent>();
        }

        [Fact]
        public void Should_verify_last_event_properties()
        {
            var childCreatedEvent = PublishedEvents.Last().As<ChildCreatedEvent>();

            childCreatedEvent.Name.Should().Be("New child");
        }
    }
}