using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using Newtonsoft.Json;

namespace EnjoyCQRS.IntegrationTests.Sqlite
{
    public class EventStoreSqlite : IEventStore
    {
        private SQLiteConnection Connection { get; set; }
        private SQLiteTransaction Transaction { get; set; }

        public string FileName { get; }
        public bool SaveSnapshotCalled { get; private set; }
        public bool GetSnapshotCalled { get; private set; }

        public EventStoreSqlite(string fileName)
        {
            FileName = fileName;

            Connection = new SQLiteConnection($"Data Source={fileName}");
        }
        
        public void BeginTransaction()
        {
            if (Transaction != null) throw new InvalidOperationException("The transaction already opened.");

            EnsureOpenedConnection();

            Transaction = Connection.BeginTransaction();
        }

        public Task CommitAsync()
        {
            if (Transaction == null) throw new InvalidOperationException("The transaction is not open.");

            Transaction.Commit();
            
            Connection.Close();

            return Task.CompletedTask;
        }

        public void Rollback()
        {
            if (Transaction?.Connection != null)
                Transaction.Rollback();
        }

        public async Task<IEnumerable<IDomainEvent>> GetAllEventsAsync(Guid id)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT Body FROM Events WHERE AggregateId = @AggregateId ORDER BY Version ASC";
            command.Parameters.AddWithValue("@AggregateId", id);

            List<IDomainEvent> events = new List<IDomainEvent>();

            EnsureOpenedConnection();

            using (command)
            using (var sqlReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (sqlReader.Read())
                {
                    events.Add(Deserialize<IDomainEvent>(sqlReader.GetString(0)));
                }
            }

            return events;
        }
        
        public async Task SaveAsync(UncommitedDomainEventCollection collection)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "INSERT INTO Events (Id, AggregateId, Timestamp, EventTypeName, Body, Version) VALUES (@Id, @AggregateId, @Timestamp, @EventTypeName, @Body, @Version)";
            command.Parameters.Add("@Id", DbType.Guid);
            command.Parameters.Add("@AggregateId", DbType.Guid);
            command.Parameters.Add("@Timestamp", DbType.DateTime);
            command.Parameters.Add("@EventTypeName", DbType.String, 250);
            command.Parameters.Add("@Body", DbType.String);
            command.Parameters.Add("@Version", DbType.Int32);

            EnsureOpenedConnection();

            using (command)
            {
                foreach (var @event in collection)
                {
                    command.Parameters[0].Value = @event.Id;
                    command.Parameters[1].Value = @event.AggregateId;
                    command.Parameters[2].Value = DateTime.UtcNow;
                    command.Parameters[3].Value = @event.GetType().Name;
                    command.Parameters[4].Value = Serialize(@event);
                    command.Parameters[5].Value = @event.Version;

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task SaveSnapshotAsync<TSnapshot>(TSnapshot snapshot) where TSnapshot : ISnapshot
        {
            var command = Connection.CreateCommand();
            command.CommandText = "INSERT INTO Snapshots (AggregateId, Timestamp, Body, Version) VALUES (@AggregateId, @Timestamp, @Body, @Version)";
            command.Parameters.Add("@AggregateId", DbType.Guid);
            command.Parameters.Add("@Timestamp", DbType.DateTime);
            command.Parameters.Add("@Body", DbType.String);
            command.Parameters.Add("@Version", DbType.Int32);

            EnsureOpenedConnection();

            command.Parameters[0].Value = snapshot.AggregateId;
            command.Parameters[1].Value = DateTime.UtcNow;
            command.Parameters[2].Value = Serialize(snapshot);
            command.Parameters[3].Value = snapshot.Version;

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            SaveSnapshotCalled = true;
        }
        
        public async Task<ISnapshot> GetSnapshotByIdAsync(Guid aggregateId)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT Body FROM Snapshots WHERE AggregateId = @AggregateId ORDER BY Version DESC LIMIT 1";
            command.Parameters.AddWithValue("@AggregateId", aggregateId);
            
            EnsureOpenedConnection();

            Snapshot snapshot = null;

            using (command)
            using (var sqlReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await sqlReader.ReadAsync())
                {
                    snapshot = Deserialize<Snapshot>(sqlReader.GetString(0));
                    break;
                }
            }

            GetSnapshotCalled = true;

            return snapshot;
        }
        
        public async Task<IEnumerable<IDomainEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT Body FROM Events WHERE AggregateId = @AggregateId AND Version > @Version ORDER BY Version ASC";
            command.Parameters.AddWithValue("@AggregateId", aggregateId);
            command.Parameters.AddWithValue("@Version", version);

            List<IDomainEvent> events = new List<IDomainEvent>();

            EnsureOpenedConnection();

            using (command)
            using (var sqlReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await sqlReader.ReadAsync())
                {
                    events.Add(Deserialize<IDomainEvent>(sqlReader.GetString(0)));
                }
            }

            return events;
        }

        public void Dispose()
        {
            Rollback();

            Connection?.Dispose();
            Transaction?.Dispose();

            Connection = null;
            Transaction = null;
        }

        private static string Serialize<TObject>(TObject @event)
        {
            return JsonConvert.SerializeObject(@event, JsonSerializerSettings);
        }

        private static TReturn Deserialize<TReturn>(string json)
        {
            var @event = JsonConvert.DeserializeObject(json, JsonSerializerSettings);

            return (TReturn)@event;
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
        };

        private void EnsureOpenedConnection()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }
    }
}