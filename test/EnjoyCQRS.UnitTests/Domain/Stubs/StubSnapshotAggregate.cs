using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs
{
    public class StubSnapshotAggregate : SnapshotAggregate<StubSnapshotAggregateSnapshot>
    {
        private readonly List<SimpleEntity> _entities = new List<SimpleEntity>();

        public string Name { get; private set; }

        public IReadOnlyList<SimpleEntity> Entities => _entities.AsReadOnly();

        private StubSnapshotAggregate(Guid newGuid, string name)
        {
            Emit(new StubAggregateCreatedEvent(newGuid, name));
        }

        public StubSnapshotAggregate()
        {
        }
        
        public static StubSnapshotAggregate Create(string name)
        {
            return new StubSnapshotAggregate(Guid.NewGuid(), name);
        }

        public void ChangeName(string name)
        {
            Emit(new NameChangedEvent(Id, name));
        }

        public void AddEntity(string entityName)
        {
            Emit(new ChildCreatedEvent(Guid.NewGuid(), entityName));
        }

        public void DisableEntity(Guid entityId)
        {
            Emit(new ChildDisabledEvent(Id, entityId));
        }
        
        protected override void RegisterEvents()
        {
            SubscribeTo<StubAggregateCreatedEvent>(x =>
            {
                Id = x.AggregateId;
                Name = x.Name;
            });

            SubscribeTo<NameChangedEvent>(x =>
            {
                Name = x.Name;
            });

            SubscribeTo<ChildCreatedEvent>(x => _entities.Add(new SimpleEntity(x.AggregateId, x.Name)));

            SubscribeTo<ChildDisabledEvent>(x =>
            {
                var entity = _entities.FirstOrDefault(e => e.Id == x.AggregateId);
                entity?.Disable();
            });
        }

        protected override StubSnapshotAggregateSnapshot CreateSnapshot()
        {
            return new StubSnapshotAggregateSnapshot
            {
                Name = Name,
                SimpleEntities = _entities
            };
        }

        protected override void RestoreFromSnapshot(StubSnapshotAggregateSnapshot snapshot)
        {
            Name = snapshot.Name;

            _entities.Clear();
            _entities.AddRange(snapshot.SimpleEntities);
        }

    }
}
