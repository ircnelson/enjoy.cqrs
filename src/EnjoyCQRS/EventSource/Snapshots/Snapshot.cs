using System;

namespace EnjoyCQRS.EventSource.Snapshots
{
    public class Snapshot
    {
        /// <summary>
        /// Unique identifier of <see cref="Aggregate"/>
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// <see cref="Aggregate.EventVersion"/>
        /// </summary>
        public int Version { get; set; }
    }
}
