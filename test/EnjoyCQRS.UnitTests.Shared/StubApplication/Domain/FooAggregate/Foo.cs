using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.FooAggregate
{
    public class Foo : SnapshotAggregate<FooSnapshot>
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public int DidSomethingCounter { get; private set; }

        public Foo()
        {
        }

        public Foo(Guid id)
        {
            Emit(new FooCreated(id));
        }

        public void ChangeName(string firstname, string lastname)
        {
            Emit(new FullNameChanged(Id, firstname, lastname));
        }

        public void DoSomething()
        {
            Emit(new DidSomething(Id));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<FooCreated>(e =>
            {
                Id = e.AggregateId;
            });

            SubscribeTo<DidSomething>(e =>
            {
                DidSomethingCounter += 1;
            });

            SubscribeTo<FullNameChanged>(e =>
            {
                FirstName = e.FirstName;
                LastName = e.LastName;
            });
        }

        protected override FooSnapshot CreateSnapshot()
        {
            return new FooSnapshot
            {
                DidSomethingCounter = DidSomethingCounter
            };
        }

        protected override void RestoreFromSnapshot(FooSnapshot snapshot)
        {
            DidSomethingCounter = snapshot.DidSomethingCounter;
        }
    }
}