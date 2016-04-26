using System;
using System.Collections.Generic;
using System.Linq;
using MyCQRS.Events;
using MyCQRS.EventStore;
using MyCQRS.EventStore.Exceptions;
using MyCQRS.EventStore.Storage;
using Newtonsoft.Json;

namespace MyCQRS.UnitTests.Storage
{
    public class InMemoryDomainEventStore : IDomainEventStore
    {
        private readonly List<IDomainEvent> _latestEvents = new List<IDomainEvent>();
        private readonly JsonSerializerSettings _serializationSettings;

        public readonly Dictionary<Guid, List<string>> EventStore = new Dictionary<Guid, List<string>>();
        public bool InTransaction;
        
        public InMemoryDomainEventStore()
        {
            _serializationSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

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

                return events.Select(Deserialize);
            }

            return Enumerable.Empty<IDomainEvent>();
        }

        public void Save(IAggregate aggregate)
        {
            var eventsToSave = aggregate.UncommitedEvents;
            var serializedEvents = eventsToSave.Select(Serialize).ToList();
            var expectedVersion = aggregate.Version - eventsToSave.Count;

            if (expectedVersion == 0)
            {
                EventStore.Add(aggregate.Id, serializedEvents);
            }
            else
            {
                var existingEvents = EventStore[aggregate.Id];
                var currentversion = existingEvents.Count;

                if (currentversion != expectedVersion)
                {
                    throw new WrongExpectedVersionException($"Expected version {expectedVersion} but the version is {currentversion}");
                }

                existingEvents.AddRange(serializedEvents);
                _latestEvents.AddRange(eventsToSave);
            }
        }

        private string Serialize(IDomainEvent domainEvent)
        {
            return JsonConvert.SerializeObject(domainEvent, _serializationSettings);
        }

        private IDomainEvent Deserialize(string domainEvent)
        {
            return JsonConvert.DeserializeObject<IDomainEvent>(domainEvent, _serializationSettings);
        }
    }
}