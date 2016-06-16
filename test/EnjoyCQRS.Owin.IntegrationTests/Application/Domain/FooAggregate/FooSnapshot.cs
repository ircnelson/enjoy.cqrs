using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.Owin.IntegrationTests.Application.Domain.FooAggregate
{
    public class FooSnapshot : Snapshot
    {
        public int DidSomethingCounter { get; set; }
    }
}