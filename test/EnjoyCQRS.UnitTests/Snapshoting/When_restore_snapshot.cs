using System;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Snapshoting
{
    public class When_restore_snapshot
    {
        public const string CategoryName = "Unit";
        public const string CategoryValue = "Snapshot";

        private StubSnapshotAggregateSnapshot _snapshot;
        private StubSnapshotAggregate _stubAggregate;

        public When_restore_snapshot()
        {
            _snapshot = new StubSnapshotAggregateSnapshot
            {
                AggregateId = Guid.NewGuid(),
                Name = "Coringa",
                Version = 1
            };

            _stubAggregate = new StubSnapshotAggregate();
            ((ISnapshotAggregate)_stubAggregate).Restore(_snapshot);
        }

        [Then]
        [Trait(CategoryName, CategoryValue)]
        public void Should_set_aggregate_properties()
        {
            _stubAggregate.Name.Should().Be(_snapshot.Name);

            _stubAggregate.Version.Should().Be(_snapshot.Version);
        }
    }
}