using System;

namespace EnjoyCQRS.EventSource.Exceptions
{
    public class AggregateNotFoundException : Exception
    {
        public string AggregateName { get; }
        public Guid AggregateId { get; }

        public AggregateNotFoundException(string aggregateName, Guid aggregateId) : base($"The aggregate '{aggregateName}' with identifier '{aggregateId}' was not found.")
        {
            AggregateName = aggregateName;
            AggregateId = aggregateId;
        }
    }
}