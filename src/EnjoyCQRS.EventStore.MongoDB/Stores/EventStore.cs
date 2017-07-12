using EnjoyCQRS.Stores;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventStore.MongoDB.Stores
{
    public class EventStore : IEventStore
    {
        private readonly IMongoDatabase _db;
        private readonly MongoEventStoreSetttings _settings;

        public EventStore(IMongoDatabase db, MongoEventStoreSetttings settings)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _settings = settings ?? new MongoEventStoreSetttings();
        }
        
        public IEventReader<TKey> GetReader<TKey>() where TKey : IEquatable<TKey>
        {
            return new MongoEventStoreReaderWriter<TKey>(_db, _settings.EventsCollectionName);
        }

        public IEventWriter<TKey> GetWriter<TKey>() where TKey : IEquatable<TKey>
        {
            return new MongoEventStoreReaderWriter<TKey>(_db, _settings.EventsCollectionName);
        }

        public Task ApplyAsync(IEnumerable<UncommitedDomainEvent> events)
        {
            throw new NotImplementedException();
        }
    }

    public class MongoEventStoreReaderWriter<TKey> : IEventWriter<TKey>, IEventReader<TKey>
        where TKey : IEquatable<TKey>
    {
        private IMongoDatabase _db;
        private readonly string _eventCollectionName;

        public MongoEventStoreReaderWriter(IMongoDatabase db, string eventCollectionName)
        {
            _db = db;
            _eventCollectionName = eventCollectionName;
        }

        public Task AppendAsync(IEnumerable<UncommitedDomainEvent> events)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDomainEvent>> GetAllAsync(TKey key)
        {
            throw new NotImplementedException();
        }
    }
}
