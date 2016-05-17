using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Snapshoting
{
    public class When_take_snapshot
    {
        public const string CategoryName = "Unit";
        public const string CategoryValue = "Snapshot";

        private StubAggregateSnapshot _snapshot;
        
        public When_take_snapshot()
        {
            var stubSnapshotAggregate = ComplexStubAggregate.Create("Superman");
            stubSnapshotAggregate.ChangeName("Batman");

            _snapshot = ((ISnapshotAggregate<StubAggregateSnapshot>) stubSnapshotAggregate).TakeSnapshot();
        }

        [Then]
        [Trait(CategoryName, CategoryValue)]
        public void Should_create_an_snapshot_object()
        {
            _snapshot.Should().BeOfType<StubAggregateSnapshot>();
        }

        [Then]
        [Trait(CategoryName, CategoryValue)]
        public void Should_verify_snapshot_properties()
        {
            _snapshot.Name.Should().Be("Batman");

            _snapshot.Version.Should().Be(2);
        }
    }
}