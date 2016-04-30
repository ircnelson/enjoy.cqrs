using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventStore.Storage;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class StubEventStore : IEventStore
    {
        private readonly List<IDomainEvent> _events = new List<IDomainEvent>();
        public IReadOnlyList<IDomainEvent> EventStore => _events.AsReadOnly();
        
        public bool InTransaction;
        
        public void BeginTransaction()
        {
            InTransaction = true;
        }

        public void Commit()
        {
            if (!InTransaction) throw new InvalidOperationException("You are not in transaction.");
            
            InTransaction = false;
        }

        public void Rollback()
        {
            InTransaction = false;
        }

        public IEnumerable<IDomainEvent> GetAllEvents(Guid id)
        {
            return _events.Where(e => e.AggregateId == id);
        }

        public void Save(IEnumerable<IDomainEvent> events)
        {
            _events.AddRange(events);
        }
    }
}