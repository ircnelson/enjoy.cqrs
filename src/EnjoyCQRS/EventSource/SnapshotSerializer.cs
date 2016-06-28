using System;
using System.Collections.Generic;
using EnjoyCQRS.Core;
using EnjoyCQRS.EventSource.Snapshots;

namespace EnjoyCQRS.EventSource
{
    public class SnapshotSerializer : ISnapshotSerializer
    {
        private readonly ITextSerializer _textSerializer;

        public SnapshotSerializer(ITextSerializer textSerializer)
        {
            _textSerializer = textSerializer;
        }

        public ISerializedSnapshot Serialize(IAggregate aggregate, ISnapshot snapshot, IEnumerable<KeyValuePair<string, string>> metadatas)
        {
            var metadata = new Metadata(metadatas);

            var aggregateId = metadata.GetValue(MetadataKeys.AggregateId, Guid.Parse);
            var aggregateVersion = metadata.GetValue(MetadataKeys.AggregateSequenceNumber, int.Parse);
            var serializedData = _textSerializer.Serialize(snapshot);
            var serializedMetadata = _textSerializer.Serialize(metadata);

            return new SerializedSnapshot(aggregateId, aggregateVersion, serializedData, serializedMetadata, metadata);
        }

        public ISnapshotRestore Deserialize(ICommitedSnapshot commitedSnapshot)
        {
            var metadata = _textSerializer.Deserialize<Metadata>(commitedSnapshot.SerializedMetadata);

            var snapshotClrType = metadata.GetValue(MetadataKeys.SnapshotClrType);

            var snapshot = (ISnapshot) _textSerializer.Deserialize(commitedSnapshot.SerializedData, snapshotClrType);

            return new SnapshotRestore(commitedSnapshot.AggregateId, commitedSnapshot.AggregateVersion, snapshot, metadata);
        }
    }
}