using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.UnitTests.Domain.AggregateWithEntities;
using EnjoyCQRS.UnitTests.Domain.Snapshots;
using FluentAssertions;

namespace EnjoyCQRS.UnitTests.Snapshoting
{
    public class When_take_snapshot
    {
        private StubAggregateSnapshot _snapshot;
        
        public When_take_snapshot()
        {
            var stubSnapshotAggregate = ComplexStubAggregate.Create("Superman");
            stubSnapshotAggregate.ChangeName("Batman");

            _snapshot = ((ISnapshotAggregate<StubAggregateSnapshot>) stubSnapshotAggregate).TakeSnapshot();
        }

        [Then]
        public void Should_create_an_snapshot_object()
        {
            _snapshot.Should().BeOfType<StubAggregateSnapshot>();
        }

        [Then]
        public void Should_verify_snapshot_properties()
        {
            _snapshot.Name.Should().Be("Batman");

            _snapshot.Version.Should().Be(2);
        }
    }
}