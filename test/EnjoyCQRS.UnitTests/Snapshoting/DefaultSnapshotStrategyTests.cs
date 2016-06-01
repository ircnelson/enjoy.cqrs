using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Snapshoting
{
    public class DefaultSnapshotStrategyTests
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "DefaultSnapshotStrategy";

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void When_aggregate_type_have_support_snapshoting()
        {
            var snapshotAggregateType = typeof(StubSnapshotAggregate);
            
            var defaultSnapshotStrategy = new DefaultSnapshotStrategy();;
            var hasSupport = defaultSnapshotStrategy.CheckSnapshotSupport(snapshotAggregateType);

            hasSupport.Should().BeTrue();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void When_aggregate_type_doesnt_have_support_snapshoting()
        {
            var snapshotAggregateType = typeof(StubAggregate);

            var defaultSnapshotStrategy = new DefaultSnapshotStrategy();
            var hasSupport = defaultSnapshotStrategy.CheckSnapshotSupport(snapshotAggregateType);

            hasSupport.Should().BeFalse();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_make_snapshot()
        {
            var defaultSnapshotStrategy = new DefaultSnapshotStrategy();

            var snapshotAggregate = Mock.Of<ISnapshotAggregate>();
            
            var makeSnapshot = defaultSnapshotStrategy.ShouldMakeSnapshot(snapshotAggregate);

            makeSnapshot.Should().BeTrue();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_not_make_snapshot()
        {
            var defaultSnapshotStrategy = new DefaultSnapshotStrategy();

            var aggregate = Mock.Of<IAggregate>();
          
            var makeSnapshot = defaultSnapshotStrategy.ShouldMakeSnapshot(aggregate);

            makeSnapshot.Should().BeFalse();
        }
    }
}