using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.UnitTests.Domain.AggregateWithEntities.Events;

namespace EnjoyCQRS.UnitTests.Domain.AggregateWithEntities
{
    public class ComplexStubAggregate : Aggregate
    {
        private readonly List<SimpleEntity> _entities = new List<SimpleEntity>();

        public string Name { get; private set; }

        public IReadOnlyList<SimpleEntity> Entities => _entities.AsReadOnly();

        private ComplexStubAggregate(Guid newGuid, string name)
        {
            Emit(new ComplexStubAggregateCreatedEvent(newGuid, name));
        }

        public ComplexStubAggregate()
        {
        }

        public static ComplexStubAggregate Create(string name)
        {
            return new ComplexStubAggregate(Guid.NewGuid(), name);
        }

        public void AddEntity(string entityName)
        {
            Emit(new ChildCreatedEvent(Guid.NewGuid(), entityName));
        }

        public void DisableEntity(Guid entityId)
        {
            Emit(new ChildDisabledEvent(entityId));
        }
        
        protected override void RegisterEvents()
        {
            SubscribeTo<ComplexStubAggregateCreatedEvent>(x =>
            {
                Id = x.AggregateId;
                Name = x.Name;
            });

            SubscribeTo<ChildCreatedEvent>(x => _entities.Add(new SimpleEntity(x.AggregateId, x.Name)));

            SubscribeTo<ChildDisabledEvent>(x =>
            {
                var entity = _entities.FirstOrDefault(e => e.Id == x.AggregateId);
                entity?.Disable();
            });
        }
    }
}
