using System.Data.SQLite;
using System.IO;

namespace EnjoyCQRS.IntegrationTests.Sqlite
{
    public class EventStoreSqliteInitializer
    {
        public string FileName { get; }

        public EventStoreSqliteInitializer(string fileName)
        {
            FileName = fileName;
        }

        public void CreateDatabase(bool force = false)
        {
            if (force && File.Exists(FileName))
                File.Delete(FileName);

            SQLiteConnection.CreateFile(FileName);
        }

        public void CreateTables()
        {
            using (var connection = new SQLiteConnection($"Data Source={FileName}"))
            {
                var command = connection.CreateCommand();

                string[] commandTexts = new[]
                {
                    @"CREATE TABLE Events (Id [uniqueidentifier] PRIMARY KEY, 
                                                             AggregateId [uniqueidentifier] NOT NULL, 
                                                             Timestamp [CURRENT_TIMESTAMP] NOT NULL, 
                                                             Metadatas [TEXT] NOT NULL, 
                                                             Body [TEXT] NOT NULL,
                                                             Version [INT])",

                    @"CREATE TABLE Snapshots (Id [uniqueidentifier] PRIMARY KEY,
                                              AggregateId [uniqueidentifier], 
                                              Timestamp [CURRENT_TIMESTAMP] NOT NULL, 
                                              SnapshotClrType [TEXT] NOT NULL,
                                              Body [TEXT] NOT NULL, 
                                              Version [INT])"
                };

                connection.Open();

                foreach (var commandText in commandTexts)
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
                
                connection.Close();
            }
        }
    }
}