using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.EventSource
{
    public abstract class SnapshotAggregate<TSnapshot> : Aggregate, ISnapshotAggregate<TSnapshot> 
        where TSnapshot : Snapshot
    {
        TSnapshot ISnapshotAggregate<TSnapshot>.TakeSnapshot()
        {
            var snapshot = CreateSnapshot();
            
            snapshot.AggregateId = Id;
            snapshot.Version = EventVersion;

            return snapshot;
        }

        void ISnapshotAggregate<TSnapshot>.Restore(TSnapshot snapshot)
        {
            Id = snapshot.AggregateId;
            Version = snapshot.Version;

            RestoreFromSnapshot(snapshot);
        }

        protected abstract TSnapshot CreateSnapshot();
        protected abstract void RestoreFromSnapshot(TSnapshot snapshot);
    }
}