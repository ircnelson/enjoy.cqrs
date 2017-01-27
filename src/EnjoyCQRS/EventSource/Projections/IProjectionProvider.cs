namespace EnjoyCQRS.EventSource.Projections
{
    public interface IProjectionProvider
    {
        object CreateProjection(IAggregate aggregate);
    }
}
