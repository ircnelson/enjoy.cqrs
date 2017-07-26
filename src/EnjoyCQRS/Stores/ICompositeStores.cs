namespace EnjoyCQRS.Stores
{
    public interface ICompositeStores
    {
        IEventStore EventStore { get; }
        ISnapshotStore SnapshotStore { get; }
    }
}
