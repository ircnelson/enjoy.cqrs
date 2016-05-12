using System;

namespace EnjoyCQRS.EventSource.Storage
{
    public class Repository : IRepository
    {
        private readonly ISession _session;
        
        public Repository(ISession session)
        {
            _session = session;
        }

        public void Add<TAggregate>(TAggregate aggregate) where TAggregate : Aggregate
        {
            _session.Add(aggregate);
        }

        public TAggregate GetById<TAggregate>(Guid id) where TAggregate : Aggregate, new()
        {
            return _session.GetById<TAggregate>(id);
        }
    }
}
