namespace EnjoyCQRS.Bus
{
    public interface ITransactionalMessageBus
    {
        void Commit();
        void Rollback();
    }
}