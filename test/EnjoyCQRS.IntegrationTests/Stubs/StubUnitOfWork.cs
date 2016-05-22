using System;
using System.Threading.Tasks;
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

        public async Task CommitAsync()
        {
            _session.BeginTransaction();

            try
            {
                await _session.SaveChangesAsync().ConfigureAwait(false);

                await _session.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                _session.Rollback();

                throw;
            }
        }
    }
}