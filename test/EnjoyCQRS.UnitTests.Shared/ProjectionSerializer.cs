using System;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;

namespace EnjoyCQRS.UnitTests.Shared
{
    public class ProjectionSerializer : IProjectionSerializer
    {
        public IProjection Serialize(IAggregate aggregate)
        {
            return new AggregateProjection(aggregate.Id, aggregate);
        }

        public class AggregateProjection : IProjection
        {
            public Guid ProjectionId { get; }

            public string Category => "aggregate";

            public object Projection { get; }

            public AggregateProjection(Guid id, object projection)
            {
                ProjectionId = id;
                Projection = projection;
            }
        }
    }
}