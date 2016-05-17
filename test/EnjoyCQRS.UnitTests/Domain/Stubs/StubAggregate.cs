using System;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.UnitTests.Domain.Stubs.Events;

namespace EnjoyCQRS.UnitTests.Domain.Stubs
{
    public class StubAggregate : Aggregate
    {
        public string Name { get; private set; }
        
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

        protected override void RegisterEvents()
        {
            SubscribeTo<NameChangedEvent>(x => { Name = x.Name; });
            SubscribeTo<StubAggregateCreatedEvent>(x =>
            {
                Id = x.AggregateId;
                Name = x.Name;
            });
        }
    }
}