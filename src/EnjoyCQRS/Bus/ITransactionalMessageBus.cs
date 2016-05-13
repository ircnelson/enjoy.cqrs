using System.Threading.Tasks;

namespace EnjoyCQRS.Bus
{
    public interface ITransactionalMessageBus
    {
        Task CommitAsync();
        void Rollback();
    }
}