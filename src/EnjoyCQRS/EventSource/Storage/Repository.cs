using System;
using System.Threading.Tasks;

namespace EnjoyCQRS.EventSource.Storage
{
    public class Repository : IRepository
    {
        private readonly ISession _session;
        
        public Repository(ISession session)
        {
            _session = session;
        }

        public async Task AddAsync<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            await _session.AddAsync(aggregate);
        }

        public Task<TAggregate> GetByIdAsync<TAggregate>(Guid id) where TAggregate : Aggregate, new()
        {
            return _session.GetByIdAsync<TAggregate>(id);
        }
    }
}
