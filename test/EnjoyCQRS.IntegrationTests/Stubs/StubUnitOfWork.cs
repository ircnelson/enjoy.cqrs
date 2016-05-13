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

        public Task CommitAsync()
        {
            try
            {
                _session.BeginTransaction();

                _session.SaveChangesAsync();

                _session.CommitAsync();
            }
            catch (Exception)
            {
                _session.Rollback();
                throw;
            }
            
            return Task.CompletedTask;
        }
    }
}