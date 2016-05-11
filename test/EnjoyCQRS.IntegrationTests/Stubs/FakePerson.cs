using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class FakePerson : Aggregate
    {
        public string Name { get; private set; }

        public FakePerson() { }

        public FakePerson(Guid id, string name) : this()
        {
            Emit(new FakePersonCreatedEvent(id, name));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<FakePersonCreatedEvent>(evt =>
            {
                Id = evt.AggregateId;
                Name = evt.Name;
            });

            SubscribeTo<NameChangedEvent>(evt =>
            {
                Name = evt.Name;
            });
        }

        public void ChangeName(string name)
        {
            Emit(new NameChangedEvent(Id, name));
        }
    }
}