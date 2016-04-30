using System;
using System.Collections.Generic;
using System.Linq;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventStore;
using EnjoyCQRS.EventStore.Exceptions;
using EnjoyCQRS.EventStore.Storage;
using Newtonsoft.Json;

namespace EnjoyCQRS.UnitTests.Storage
{
    public class StubEventStore : IEventStore
    {
        public readonly Dictionary<Guid, List<IDomainEvent>> EventStore = new Dictionary<Guid, List<IDomainEvent>>();
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
            if (EventStore.ContainsKey(id))
            {
                var events = EventStore[id];

                return events;
            }

            return Enumerable.Empty<IDomainEvent>();
        }
        
        public void Save(IEnumerable<IDomainEvent> events)
        {
            foreach (var @event in events)
            {
                var aggregateId = @event.AggregateId;

                List<IDomainEvent> list;
                EventStore.TryGetValue(aggregateId, out list);
                if (list == null)
                {
                    list = new List<IDomainEvent>();
                    EventStore.Add(aggregateId, list);
                }
                list.Add(@event);
            }
            
            //else
            //{
            //    var existingEvents = EventStore[aggregateId];
            //    var currentversion = existingEvents.Count;

            //    if (currentversion != expectedVersion)
            //    {
            //        throw new WrongExpectedVersionException($"Expected version {expectedVersion} but the version is {currentversion}");
            //    }

            //    existingEvents.AddRange(events);
            //}
        }
    }
}