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

        public void CreateTable()
        {
            using (var connection = new SQLiteConnection($"Data Source={FileName}"))
            {
                var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE Events (Id [uniqueidentifier] PRIMARY KEY, 
                                                             AggregateId [uniqueidentifier] NOT NULL, 
                                                             Timestamp [CURRENT_TIMESTAMP] NOT NULL, 
                                                             EventTypeName [VARCHAR(250)] NOT NULL, 
                                                             Body [TEXT] NOT NULL,
                                                             Version [INT])";

                connection.Open();

                command.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}