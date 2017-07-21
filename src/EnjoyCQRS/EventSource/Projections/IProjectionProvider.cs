namespace EnjoyCQRS.EventSource.Projections
{
    public interface IProjectionProvider
    {
        IProjection CreateProjection(IAggregate aggregate);
    }
}
