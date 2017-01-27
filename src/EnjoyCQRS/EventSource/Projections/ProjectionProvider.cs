namespace EnjoyCQRS.EventSource.Projections
{
    public abstract class ProjectionProvider<TAggregate, TProjection> : IProjectionProvider
        where TAggregate : IAggregate
        where TProjection : class, new()
    {
        public abstract TProjection CreateProjection(TAggregate aggregate);

        object IProjectionProvider.CreateProjection(IAggregate aggregate)
        {
            return CreateProjection((TAggregate)aggregate);
        }
    }
}
