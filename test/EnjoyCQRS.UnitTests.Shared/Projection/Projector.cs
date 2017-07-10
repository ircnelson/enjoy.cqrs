using EnjoyCQRS.Projections;

namespace EnjoyCQRS.UnitTests.Shared.Projection
{
    public abstract class Projector<TKey, TView>
        where TView : new()
    {
        protected IDocumentWriter<TKey, TView> Store { get; }
        
        protected Projector(IDocumentWriter<TKey, TView> store)
        {
            Store = store;
        }
    }
}
