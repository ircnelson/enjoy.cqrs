using System;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.EventSource
{
    /// <summary>
    /// Default implementation of <see cref="IUnitOfWork"/>.
    /// Keep the session in transaction state and rollback if do something wrong.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISession _session;

        public UnitOfWork(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Start transaction, saves the session and commit.
        /// Rollback will be called if something was wrong.
        /// </summary>
        /// <returns></returns>
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