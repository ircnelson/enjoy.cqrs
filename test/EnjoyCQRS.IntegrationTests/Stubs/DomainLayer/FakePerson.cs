using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.IntegrationTests.Stubs.DomainLayer
{
    public class FakePerson : Aggregate
    {
        public string Name { get; private set; }

        public FakePerson() { }

        public FakePerson(Guid id, string name) : this()
        {
            Emit(new FakePersonCreated(id, name));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<FakePersonCreated>(evt =>
            {
                Id = evt.AggregateId;
                Name = evt.Name;
            });

            SubscribeTo<NameChanged>(evt =>
            {
                Name = evt.Name;
            });
        }

        public void ChangeName(string name)
        {
            Emit(new NameChanged(Id, name));
        }
    }
}