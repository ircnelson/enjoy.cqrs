using EnjoyCQRS.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EnjoyCQRS.Stores
{
    public interface IEventStore
    {
        IEventReader<TKey> GetReader<TKey>() where TKey : IEquatable<TKey>;
        IEventWriter<TKey> GetWriter<TKey>() where TKey : IEquatable<TKey>;
        Task ApplyAsync(IEnumerable<UncommitedDomainEvent> events);
    }

    public interface IEventReader<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<IEnumerable<IDomainEvent>> GetAllAsync(TKey key);
    }

    public interface IEventWriter<TKey>
        where TKey : IEquatable<TKey>
    {
        Task AppendAsync(IEnumerable<UncommitedDomainEvent> events);
    }

    //public interface IEnvelopeSerializer
    //{
    //    void Serialize(EnvelopedEvent envelopedEvent, Stream stream);
    //    IDomainEvent DeserializeEvent(Stream stream);
    //}


    public interface IEventSerializer
    {
        byte[] Serialize(IDomainEvent @event, Type eventType, IEnumerable<KeyValuePair<string, object>> metadatas);
        IDomainEvent Deserialize(byte[] commitedEvent);
    }

    public class UncommitedDomainEvent
    {
        public readonly Dictionary<string, object> Metadata = new Dictionary<string, object>();
        public Guid AggregateId { get; private set; }
        public IDomainEvent Data { get; }

        public UncommitedDomainEvent(Guid aggregateId, IDomainEvent data)
        {
            AggregateId = aggregateId;
            Data = data;
        }
    }

    public class MemoryEventStore : IEventStore
    {
        private readonly IEventSerializer _serializer;
        private readonly ConcurrentBag<EnvelopedEvent> _store;

        public MemoryEventStore(ConcurrentBag<EnvelopedEvent> store)
        {
            store = store ?? throw new ArgumentNullException(nameof(store));

            _store = store;
        }

        public IEventReader<TKey> GetReader<TKey>()
            where TKey : IEquatable<TKey>
        {
            return new MemoryEventStoreReaderWriter<TKey>(_store);
        }

        public IEventWriter<TKey> GetWriter<TKey>()
            where TKey : IEquatable<TKey>
        {
            return new MemoryEventStoreReaderWriter<TKey>(_store);
        }

        public Task<IEnumerable<UncommitedDomainEvent>> EnumerateContentsAsync(string bucket)
        {
            throw new NotImplementedException();
        }
        public Task ApplyAsync(IEnumerable<UncommitedDomainEvent> events)
        {
            throw new NotImplementedException();
        }
    }

    public class MemoryEventStoreReaderWriter<TKey> : IEventWriter<TKey>, IEventReader<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ConcurrentBag<EnvelopedEvent> _store = new ConcurrentBag<EnvelopedEvent>();
        private readonly IEventSerializer _serializer;

        public MemoryEventStoreReaderWriter()
        {
        }

        public MemoryEventStoreReaderWriter(ConcurrentBag<EnvelopedEvent> store)
        {
            _store = store;
        }

        public Task AppendAsync(IEnumerable<UncommitedDomainEvent> records)
        {
            foreach (var record in records)
            {
                _store.Add(new EnvelopedEvent(record.AggregateId, record.Data, record.Metadata));
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<IDomainEvent>> GetAllAsync(TKey key)
        {
            var events = _store
               .Where(e => e.AggregateId.Equals(key))
               .ToList()
               .Select(e => e.Data)
               .Cast<IDomainEvent>();
            
            return Task.FromResult<IEnumerable<IDomainEvent>>(events);
        }
    }

    public sealed class EnvelopedEvent
    {
        public readonly DateTime CreatedAt = DateTime.Now;
        public Guid AggregateId { get; }
        public object Data { get; private set; }
        public Dictionary<string, object> Metadata { get; private set; }

        public EnvelopedEvent(Guid aggregateId, object data, Dictionary<string, object> metadata)
        {
            AggregateId = aggregateId;
            Data = data;
            Metadata = metadata;
        }
    }
}
