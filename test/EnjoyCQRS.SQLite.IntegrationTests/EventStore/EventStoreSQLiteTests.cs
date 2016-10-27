using System.IO;
using System.Threading.Tasks;
using EnjoyCQRS.EventStore.SQLite;
using EnjoyCQRS.UnitTests.Shared.TestSuit;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.SQLite.IntegrationTests.EventStore
{
    public class EventStoreSQLiteTests : IClassFixture<DatabaseFixtures>
    {
        public const string CategoryName = "Integration";
        public const string CategoryValue = "SQLite";

        private readonly DatabaseFixtures _fixtures;
        private readonly string _fileName;

        public EventStoreSQLiteTests(DatabaseFixtures fixtures)
        {
            _fixtures = fixtures;

            _fileName = _fixtures.EventStoreInitializer.FileName;
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_create_database()
        {
            File.Exists(_fixtures.EventStoreInitializer.FileName).Should().BeTrue();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Test_events()
        {
            var eventStore = new EventStoreSqlite(_fileName);
            
            var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

            await eventStoreTestSuit.EventTestsAsync();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task Test_snapshot()
        {
            var eventStore = new EventStoreSqlite(_fileName);
            var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

            await eventStoreTestSuit.SnapshotTestsAsync();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public async Task When_any_exception_be_thrown()
        {
            var eventStore = new EventStoreSqlite(_fileName);
            var eventStoreTestSuit = new EventStoreTestSuit(eventStore);

            await eventStoreTestSuit.DoSomeProblemAsync();
        }
    }

    public class DatabaseFixtures
    {
        public EventStoreSqliteInitializer EventStoreInitializer { get; }

        public DatabaseFixtures()
        {
            EventStoreInitializer = new EventStoreSqliteInitializer("test.db");
            EventStoreInitializer.CreateDatabase(true);
            EventStoreInitializer.CreateTables();
        }

    }
}