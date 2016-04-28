namespace EnjoyCQRS.Bus
{
    public interface IUnitOfWork
    {
        void Commit();
        void Rollback();
    }
}