using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FooAggregate
{
    public class FooSnapshot : Snapshot
    {
        public int DidSomethingCounter { get; set; }
    }
}