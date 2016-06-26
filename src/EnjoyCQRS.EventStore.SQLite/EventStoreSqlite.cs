using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.EventStore.SQLite
{
    public class EventStoreSqlite : IEventStore
    {   
        private SQLiteConnection Connection { get; set; }
        private SQLiteTransaction Transaction { get; set; }
        
        public EventStoreSqlite(string fileName)
        {
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

        public async Task<IEnumerable<ICommitedEvent>> GetAllEventsAsync(Guid id)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT AggregateId, Version, Body, Metadatas FROM Events WHERE AggregateId = @AggregateId ORDER BY Version ASC";
            command.Parameters.AddWithValue("@AggregateId", id);

            return await GetEventsAsync(command);
        }
        
        public async Task SaveAsync(IEnumerable<ISerializedEvent> collection)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "INSERT INTO Events (Id, AggregateId, Version, Timestamp, Body, Metadatas) VALUES (@Id, @AggregateId, @Version, @Timestamp, @Body, @Metadatas)";
            command.Parameters.Add("@Id", DbType.Guid);
            command.Parameters.Add("@AggregateId", DbType.Guid);
            command.Parameters.Add("@Version", DbType.Int32);
            command.Parameters.Add("@Timestamp", DbType.DateTime);
            command.Parameters.Add("@Body", DbType.String);
            command.Parameters.Add("@Metadatas", DbType.String);

            EnsureOpenedConnection();

            using (command)
            {
                foreach (var @event in collection)
                {
                    command.Parameters[0].Value = @event.Metadata.GetValue(MetadataKeys.EventId, Guid.Parse);
                    command.Parameters[1].Value = @event.Metadata.GetValue(MetadataKeys.AggregateId, Guid.Parse);
                    command.Parameters[2].Value = @event.Metadata.GetValue(MetadataKeys.EventVersion, int.Parse);
                    command.Parameters[3].Value = DateTime.UtcNow;
                    command.Parameters[4].Value = @event.SerializedData;
                    command.Parameters[5].Value = @event.SerializedMetadata;

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
        }

        public async Task SaveSnapshotAsync(ISerializedSnapshot snapshot)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "INSERT INTO Snapshots (AggregateId, Version, Timestamp, Body, Metadatas) VALUES (@AggregateId, @Version, @Timestamp, @Body, @Metadatas)";
            command.Parameters.Add("@AggregateId", DbType.Guid);
            command.Parameters.Add("@Version", DbType.Int32);
            command.Parameters.Add("@Timestamp", DbType.DateTime);
            command.Parameters.Add("@Body", DbType.String);
            command.Parameters.Add("@Metadatas", DbType.String);

            EnsureOpenedConnection();

            command.Parameters[0].Value = snapshot.AggregateId;
            command.Parameters[1].Value = snapshot.AggregateVersion;
            command.Parameters[2].Value = DateTime.UtcNow;
            command.Parameters[3].Value = snapshot.SerializedData; 
            command.Parameters[4].Value = snapshot.SerializedMetadata;

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        
        public async Task<ICommitedSnapshot> GetLatestSnapshotByIdAsync(Guid aggregateId)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT AggregateId, Version, Body, Metadatas FROM Snapshots WHERE AggregateId = @AggregateId ORDER BY Version DESC LIMIT 1";
            command.Parameters.AddWithValue("@AggregateId", aggregateId);
            
            EnsureOpenedConnection();

            ICommitedSnapshot snapshot = null;

            using (command)
            using (var sqlReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await sqlReader.ReadAsync())
                {
                    snapshot = new SqliteCommitedSnapshot(sqlReader.GetGuid(0), sqlReader.GetInt32(1), sqlReader.GetString(2), sqlReader.GetString(3));
                    break;
                }
            }
            
            return snapshot;
        }
        
        public async Task<IEnumerable<ICommitedEvent>> GetEventsForwardAsync(Guid aggregateId, int version)
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT AggregateId, Version, Body, Metadatas FROM Events WHERE AggregateId = @AggregateId AND Version > @Version ORDER BY Version ASC";
            command.Parameters.AddWithValue("@AggregateId", aggregateId);
            command.Parameters.AddWithValue("@Version", version);

            return await GetEventsAsync(command);
        }

        public void Dispose()
        {
            Rollback();

            Connection?.Dispose();
            Transaction?.Dispose();

            Connection = null;
            Transaction = null;
        }
        
        private void EnsureOpenedConnection()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        private async Task<IEnumerable<ICommitedEvent>> GetEventsAsync(SQLiteCommand command)
        {
            List<ICommitedEvent> events = new List<ICommitedEvent>();

            EnsureOpenedConnection();

            using (command)
            using (var sqlReader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (await sqlReader.ReadAsync())
                {
                    events.Add(new SqliteCommitedEvent(sqlReader.GetGuid(0), sqlReader.GetInt32(1), sqlReader.GetString(2), sqlReader.GetString(3)));
                }
            }

            return events;
        }

        public class SqliteCommitedEvent : ICommitedEvent
        {
            public SqliteCommitedEvent(Guid aggregateId, int aggregateVersion, string serializedData, string serializedMetadata)
            {
                AggregateId = aggregateId;
                AggregateVersion = aggregateVersion;
                SerializedData = serializedData;
                SerializedMetadata = serializedMetadata;
            }

            public Guid AggregateId { get; }
            public int AggregateVersion { get; }
            public string SerializedMetadata { get; }
            public string SerializedData { get; }
        }

        public class SqliteCommitedSnapshot : ICommitedSnapshot
        {
            public SqliteCommitedSnapshot(Guid aggregateId, int aggregateVersion, string serializedData, string serializedMetadata)
            {
                AggregateId = aggregateId;
                AggregateVersion = aggregateVersion;
                SerializedData = serializedData;
                SerializedMetadata = serializedMetadata;
            }

            public Guid AggregateId { get; }
            public int AggregateVersion { get; }
            public string SerializedMetadata { get; }
            public string SerializedData { get; }
        }
    }
}