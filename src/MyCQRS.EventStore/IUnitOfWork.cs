namespace MyCQRS.EventStore
{
    public interface IUnitOfWork
    {
        void Commit();
        void Rollback();
    }
}