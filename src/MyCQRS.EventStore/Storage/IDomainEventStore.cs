using MyCQRS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCQRS.EventStore.Storage
{
    public interface IDomainEventStore<TDomainEvent> : ITransactional
        where TDomainEvent : IDomainEvent
    {
        IEnumerable<TDomainEvent> GetAllEvents(Guid aggregateId);
        void Save(IAggregate aggregate);
    }
}
