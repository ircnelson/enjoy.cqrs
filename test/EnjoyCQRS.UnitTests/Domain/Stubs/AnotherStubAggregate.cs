using System;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs
{
    public class AnotherStubAggregate : Aggregate
    {
        public string Name { get; private set; }
        public Guid StubAggregateId { get; private set; }

        public AnotherStubAggregate()
        {
        }

        private AnotherStubAggregate(Guid newGuid, string name)
        {
            Emit(new StubAggregateCreatedEvent(newGuid, name));
        }

        public static AnotherStubAggregate Create(string name)
        {
            return new AnotherStubAggregate(Guid.NewGuid(), name);
        }

        public void ChangeName(string name)
        {
            Emit(new NameChangedEvent(Id, name));
        }

        public void Relationship(Guid stubAggregateId)
        {
            Emit(new StubAggregateRelatedEvent(Id, stubAggregateId));
        }
        
        protected override void RegisterEvents()
        {
            SubscribeTo<NameChangedEvent>(x => { Name = x.Name; });

            SubscribeTo<StubAggregateRelatedEvent>(x =>
            {
                StubAggregateId = x.StubAggregateId;
            });

            SubscribeTo<StubAggregateCreatedEvent>(x =>
            {
                Id = x.AggregateId;
                Name = x.Name;
            });
        }
    }
}