using System;
using System.Collections.Generic;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.UnitTests.Domain.AggregateWithEntities;
using EnjoyCQRS.UnitTests.Domain.Events;
using EnjoyCQRS.UnitTests.Domain.Snapshots;

namespace EnjoyCQRS.UnitTests.Domain
{
    public class StubAggregateWithSnapshot : SnapshotAggregate<StubAggregateSnapshot>
    {
        private readonly List<SimpleEntity> _simpleEntities = new List<SimpleEntity>();

        public string Name { get; private set; }
        public IReadOnlyList<SimpleEntity> SimpleEntities => _simpleEntities.AsReadOnly();

        private StubAggregateWithSnapshot(Guid newGuid)
        {
            Emit(new TestAggregateCreatedEvent(newGuid));
        }

        public StubAggregateWithSnapshot()
        {
        }

        public static StubAggregateWithSnapshot Create()
        {
            return new StubAggregateWithSnapshot(Guid.NewGuid());
        }

        public void ChangeName(string name)
        {
            Emit(new SomeEvent(Id, name));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<SomeEvent>(x => { Name = x.Name; });
            SubscribeTo<TestAggregateCreatedEvent>(x => { Id = x.AggregateId; });
        }

        protected override StubAggregateSnapshot CreateSnapshot()
        {
            return new StubAggregateSnapshot
            {
                Name = Name,
                SimpleEntities = _simpleEntities
            };
        }

        protected override void RestoreFromSnapshot(StubAggregateSnapshot snapshot)
        {
            Name = snapshot.Name;

            _simpleEntities.Clear();
            _simpleEntities.AddRange(snapshot.SimpleEntities);
        }
    }
}