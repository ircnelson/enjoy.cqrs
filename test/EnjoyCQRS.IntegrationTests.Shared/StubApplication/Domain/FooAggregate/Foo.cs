using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FooAggregate
{
    public class Foo : SnapshotAggregate<FooSnapshot>
    {
        public int DidSomethingCounter { get; private set; }

        public Foo()
        {
        }

        public Foo(Guid id)
        {
            Emit(new FooCreated(id));
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