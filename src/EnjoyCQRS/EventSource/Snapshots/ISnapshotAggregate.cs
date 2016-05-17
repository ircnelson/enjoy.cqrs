namespace EnjoyCQRS.EventSource.Snapshots
{
    public interface ISnapshotAggregate<TSnapshot>
        where TSnapshot : Snapshot
    {
        TSnapshot TakeSnapshot();
        void Restore(TSnapshot snapshot);
    }
}