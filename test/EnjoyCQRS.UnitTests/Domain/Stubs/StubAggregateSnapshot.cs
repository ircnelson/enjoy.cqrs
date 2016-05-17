using System.Collections.Generic;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.UnitTests.Domain.AggregateWithEntities;

namespace EnjoyCQRS.UnitTests.Domain.Snapshots
{
    public class StubAggregateSnapshot : Snapshot
    {
        public string Name { get; set; }
        public List<SimpleEntity> SimpleEntities { get; set; } = new List<SimpleEntity>();
    }
}