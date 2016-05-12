using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class StubUnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;

        public StubUnitOfWork(ISession session)
        {
            _session = session;
        }

        public void Commit()
        {
            _session.BeginTransaction();

            _session.SaveChanges();

            _session.Commit();
        }

        public void Rollback()
        {
            _session.Rollback();
        }
    }
}