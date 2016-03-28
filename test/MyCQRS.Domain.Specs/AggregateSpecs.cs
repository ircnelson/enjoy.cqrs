using FluentAssertions;
using MyCQRS.Domain.Specs.Stubs;
using Xunit;

namespace MyCQRS.Domain.Specs
{
    public class AggregateSpecs
    {
        private AggregateStub Subject;

        public AggregateSpecs()
        {
            Subject = new AggregateStub();
        }

        [Fact]
        public void State_should_be_modified()
        {
            Subject.ChangeName("Walter White");

            Subject.Name.Should().Be("Walter White");
        }

        [Fact]
        public void The_version_of_new_aggregate_instance()
        {
            Subject.Version.Should().Be(-1);
        }

        [Fact]
        public void Version_should_be_modified()
        {
            Subject.ChangeName("Walter White");
            Subject.ChangeName("Heisenberg");

            Subject.Version.Should().Be(1);
        }

        [Fact]
        public void New_events_should_be_added_in_uncommited_event_list()
        {
            Subject.Activate();
            Subject.ChangeName("Walter White");
            Subject.ChangeName("Heisenberg");

            Subject.UncommitedEvents.Should().HaveCount(3);
        }

        [Fact]
        public void Clearing_uncommited_events()
        {
            Subject.Activate();
            Subject.ChangeName("Walter White");
            Subject.ChangeName("Heisenberg");

            Subject.ClearUncommitedEvents();

            Subject.UncommitedEvents.Should().HaveCount(0);
        }
    }
}