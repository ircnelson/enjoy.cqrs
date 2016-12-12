using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.Extensions
{
    internal static class AggregateExtensions
    {
        public static async Task TakeSnapshot(this Aggregate aggregate, 
            IEventStore eventStore, 
            ISnapshotSerializer snapshotSerializer)
        {
            var snapshot = ((ISnapshotAggregate)aggregate).CreateSnapshot();

            var metadatas = new[]
            {
                new KeyValuePair<string, string>(MetadataKeys.AggregateId, aggregate.Id.ToString()),
                new KeyValuePair<string, string>(MetadataKeys.AggregateSequenceNumber, aggregate.Sequence.ToString()),
                new KeyValuePair<string, string>(MetadataKeys.SnapshotId, Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>(MetadataKeys.SnapshotClrType, snapshot.GetType().AssemblyQualifiedName),
                new KeyValuePair<string, string>(MetadataKeys.SnapshotName, snapshot.GetType().Name),
            };

            var serializedSnapshot = snapshotSerializer.Serialize(aggregate, snapshot, metadatas);

            await eventStore.SaveSnapshotAsync(serializedSnapshot).ConfigureAwait(false);
        }

        public static IEnumerable<ISerializedEvent> ToSerialized(this IAggregate aggregate, 
            IEnumerable<IMetadataProvider> metadataProviders, 
            IEventSerializer eventSerializer)
        {
            var serializedEvents = aggregate.UncommitedEvents.Select((e, index) =>
            {
                index++;

                var metadatas =
                    metadataProviders.SelectMany(md => md.Provide(aggregate, e.OriginalEvent, Metadata.Empty)).Concat(new[]
                    {
                        new KeyValuePair<string, string>(MetadataKeys.EventId, Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string>(MetadataKeys.EventVersion, (aggregate.Version + index).ToString())
                    });

                return eventSerializer.Serialize(aggregate, e.OriginalEvent, new Metadata(metadatas));
            });

            return serializedEvents;
        }
    }
}