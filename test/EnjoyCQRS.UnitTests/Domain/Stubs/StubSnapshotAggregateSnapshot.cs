using System.Collections.Generic;
using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.UnitTests.Domain.Stubs
{
    public class StubSnapshotAggregateSnapshot : Snapshot
    {
        public string Name { get; set; }
        public List<SimpleEntity> SimpleEntities { get; set; } = new List<SimpleEntity>();
    }
}