using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource.Storage;
using Newtonsoft.Json;

namespace EnjoyCQRS.IntegrationTests.Sqlite
{
    public class EventStoreSqlite : IEventStore
    {
        public string FileName { get; }
        private SQLiteConnection Connection { get; set; }
        private SQLiteTransaction Transaction { get; set; }

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

        public Task<IEnumerable<IDomainEvent>> GetAllEventsAsync(Guid id)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT Body FROM Events WHERE AggregateId = @AggregateId ORDER BY Version ASC";
            command.Parameters.AddWithValue("@AggregateId", id);

            List<IDomainEvent> events = new List<IDomainEvent>();

            EnsureOpenedConnection();

            using (command)
            using (var sqlReader = command.ExecuteReader())
            {
                while (sqlReader.Read())
                {
                    events.Add(Deserialize(sqlReader.GetString(0)));
                }
            }

            return Task.FromResult(events.AsEnumerable());
        }
        
        public Task SaveAsync(IEnumerable<IDomainEvent> events)
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
                foreach (var @event in events)
                {
                    command.Parameters[0].Value = @event.Id;
                    command.Parameters[1].Value = @event.AggregateId;
                    command.Parameters[2].Value = DateTime.UtcNow;
                    command.Parameters[3].Value = @event.GetType().Name;
                    command.Parameters[4].Value = Serialize(@event);
                    command.Parameters[5].Value = @event.Version;

                    command.ExecuteNonQuery();
                }
            }

            return Task.CompletedTask;
        }
        
        private static string Serialize(IDomainEvent @event)
        {
            return JsonConvert.SerializeObject(@event, JsonSerializerSettings);
        }

        private static IDomainEvent Deserialize(string json)
        {
            var @event = JsonConvert.DeserializeObject(json, JsonSerializerSettings);

            return (IDomainEvent) @event;
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

        public void Dispose()
        {
            Rollback();

            Connection?.Dispose();
            Transaction?.Dispose();

            Connection = null;
            Transaction = null;
        }
    }
}