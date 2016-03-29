using System;
using System.Linq;
using FluentAssertions;
using MyCQRS.CommandHandlers;
using MyCQRS.Commands;
using MyCQRS.Domain.StubBoundedContext;
using MyCQRS.Domain.StubBoundedContext.Events;
using Xunit;

namespace MyCQRS.Domain.Specs.Commands
{
    public class ChangeNameCommandTest : CommandTestFixture<ChangeNameCommand, ChangeNameCommandHandler, AggregateStub>
    {
        protected override ChangeNameCommand When()
        {
            return new ChangeNameCommand(Guid.NewGuid(), "Walter White");
        }

        [Fact]
        public void Then_the_event_should_be_published()
        {
            PublishedEvents.Last().Should().BeAssignableTo<ChangedNameEvent>();
        }

        [Fact]
        public void Then_the_event_Name_property()
        {
            PublishedEvents.Last().As<ChangedNameEvent>().Name.Should().Be("Walter White");
        }
    }
}