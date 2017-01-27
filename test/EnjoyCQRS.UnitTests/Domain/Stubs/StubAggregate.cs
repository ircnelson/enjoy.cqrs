using System;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;
using EnjoyCQRS.Attributes;
using EnjoyCQRS.EventSource.Projections;

namespace EnjoyCQRS.UnitTests.Domain.Stubs
{
    [ProjectionProvider(typeof(StubAggregateProjectionProvider))]
    public class StubAggregate : Aggregate
    {
        public string Name { get; private set; }
        public Guid RelatedId { get; private set; }

        private StubAggregate(Guid newGuid, string name)
        {
            Emit(new StubAggregateCreatedEvent(newGuid, name));
        }

        public StubAggregate()
        {
        }

        public static StubAggregate Create(string name)
        {
            return new StubAggregate(Guid.NewGuid(), name);
        }
        
        public void ChangeName(string name)
        {
            Emit(new NameChangedEvent(Id, name));
        }

        public void DoSomethingWithoutEventSubscription()
        {
            Emit(new NotRegisteredEvent(Id));
        }

        public void Relationship(Guid relatedId)
        {
            Emit(new StubAggregateRelatedEvent(Id, relatedId));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<NameChangedEvent>(x => { Name = x.Name; });
            SubscribeTo<StubAggregateCreatedEvent>(x =>
            {
                Id = x.AggregateId;
                Name = x.Name;
            });

            SubscribeTo<StubAggregateRelatedEvent>(x =>
            {
                RelatedId = x.StubAggregateId;
            });
        }
        
    }

    public class StubAggregateProjectionProvider : IProjectionProvider
    {
        public object CreateProjection(IAggregate aggregate)
        {
            return aggregate;
        }
    }
}