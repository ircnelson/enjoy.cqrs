using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Snapshoting
{
    public class When_create_snapshot
    {
        public const string CategoryName = "Unit";
        public const string CategoryValue = "Snapshot";

        private readonly ISnapshot _snapshot;
        
        public When_create_snapshot()
        {
            var stubSnapshotAggregate = StubSnapshotAggregate.Create("Superman");
            stubSnapshotAggregate.ChangeName("Batman");
            stubSnapshotAggregate.AddEntity("entity 1");
            var entityId = stubSnapshotAggregate.AddEntity("entity 2");

            stubSnapshotAggregate.DisableEntity(entityId);

            _snapshot = ((ISnapshotAggregate) stubSnapshotAggregate).CreateSnapshot();
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_create_an_snapshot_object()
        {
            _snapshot.Should().BeOfType<StubSnapshotAggregateSnapshot>();
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_verify_snapshot_properties()
        {
            var snapshot = (StubSnapshotAggregateSnapshot) _snapshot;

            snapshot.Name.Should().Be("Batman");
            snapshot.SimpleEntities.Count.Should().Be(2);
            snapshot.SimpleEntities[1].Enabled.Should().BeFalse();
        }
    }
}