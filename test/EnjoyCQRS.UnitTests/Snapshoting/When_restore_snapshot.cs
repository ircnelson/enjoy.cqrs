using System;
using System.Collections.Generic;
using EnjoyCQRS.EventSource;
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

        private readonly StubSnapshotAggregateSnapshot _snapshot;
        private readonly StubSnapshotAggregate _stubAggregate;

        public When_restore_snapshot()
        {
            var aggregateId = Guid.NewGuid();
            var version = 1;

            _snapshot = new StubSnapshotAggregateSnapshot
            {
                Name = "Coringa",
            };
            
            _stubAggregate = new StubSnapshotAggregate();
            ((ISnapshotAggregate)_stubAggregate).Restore(new SnapshotRestore(aggregateId, version, _snapshot, EventSource.Metadata.Empty));
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_set_aggregate_properties()
        {
            _stubAggregate.Name.Should().Be(_snapshot.Name);

            _stubAggregate.Version.Should().Be(1);
        }
    }
}