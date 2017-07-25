using EnjoyCQRS.EventSource.Storage;
using IProjectionStoreV1 = EnjoyCQRS.EventSource.Projections.IProjectionStore;

namespace EnjoyCQRS.Stores
{
    public interface ICompositeStores
    {
        IEventStore EventStore { get; }
        ISnapshotStore SnapshotStore { get; }
        IProjectionStoreV1 ProjectionStoreV1 { get; }
    }
}
