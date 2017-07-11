using EnjoyCQRS.Projections;

namespace EnjoyCQRS.UnitTests.Shared.Projection
{
    public abstract class Projector<TKey, TView>
        where TView : new()
    {
        protected IProjectionWriter<TKey, TView> Store { get; }
        
        protected Projector(IProjectionWriter<TKey, TView> store)
        {
            Store = store;
        }
    }
}
