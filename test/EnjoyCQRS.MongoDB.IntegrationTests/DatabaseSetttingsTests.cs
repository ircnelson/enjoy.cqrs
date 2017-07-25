using EnjoyCQRS.EventStore.MongoDB;
using EnjoyCQRS.EventStore.MongoDB.Stores;
using FluentAssertions;
using MongoDB.Driver;
using System;
using Xunit;

namespace EnjoyCQRS.MongoDB.IntegrationTests
{
    [Trait("Integration", "MongoDB")]
    public class DatabaseSetttingsTests
    {
        [Theory]
        [InlineData("events", null)]
        [InlineData(null, "snapshots")]
        public void Should_validate_settings(string eventCollectionName, string snapshotCollectionName)
        {
            var defaultSettings = new MongoEventStoreSetttings
            {
                EventsCollectionName = eventCollectionName,
                SnapshotsCollectionName = snapshotCollectionName
            };

            Action action = () => new MongoStores(new MongoClient(), "db", defaultSettings);

            action.ShouldThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void Should_use_custom_settings()
        {
            var customSettings = new MongoEventStoreSetttings
            {
                EventsCollectionName = "MyEvents",
                SnapshotsCollectionName = "MySnapshots"
            };

            using (var eventStore = new MongoStores(new MongoClient(), "db", customSettings))
            {
                eventStore.Settings.EventsCollectionName.Should().Be(customSettings.EventsCollectionName);
                eventStore.Settings.SnapshotsCollectionName.Should().Be(customSettings.SnapshotsCollectionName);
            }
        }

        [Fact]
        public void Should_use_default_settings()
        {
            var defaultSettings = new MongoEventStoreSetttings();

            using (var eventStore = new MongoStores(new MongoClient(), "db"))
            {
                eventStore.Settings.EventsCollectionName.Should().Be(defaultSettings.EventsCollectionName);
                eventStore.Settings.SnapshotsCollectionName.Should().Be(defaultSettings.SnapshotsCollectionName);
            }
        }
    }
}
