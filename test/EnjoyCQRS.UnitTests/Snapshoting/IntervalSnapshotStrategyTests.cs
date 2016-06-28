using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.UnitTests.Domain.Stubs;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnjoyCQRS.UnitTests.Snapshoting
{
    public class IntervalSnapshotStrategyTests
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "IntervalSnapshotStrategy";

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void When_aggregate_type_have_support_snapshoting()
        {
            var snapshotAggregateType = typeof(StubSnapshotAggregate);
            
            var itervalSnapshotStrategy = new IntervalSnapshotStrategy();
            var hasSupport = itervalSnapshotStrategy.CheckSnapshotSupport(snapshotAggregateType);

            hasSupport.Should().BeTrue();
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void When_aggregate_type_doesnt_have_support_snapshoting()
        {
            var snapshotAggregateType = typeof(StubAggregate);

            var defaultSnapshotStrategy = new IntervalSnapshotStrategy();
            var hasSupport = defaultSnapshotStrategy.CheckSnapshotSupport(snapshotAggregateType);

            hasSupport.Should().BeFalse();
        }

        [Trait(CategoryName, CategoryValue)]
        [Theory]
        [InlineData(100, 100, true)]
        [InlineData(200, 100, true)]
        [InlineData(101, 100, false)]
        [InlineData(115, 15, false)]
        [InlineData(105, 15, true)]
        [InlineData(2, 5, false)]
        public void Should_make_snapshot(int aggregateEventVersion, int snapshotInterval, bool expected)
        {
            var itervalSnapshotStrategy = new IntervalSnapshotStrategy(snapshotInterval);

            var snapshotAggregateMock = new Mock<ISnapshotAggregate>();
            snapshotAggregateMock.Setup(e => e.Sequence).Returns(aggregateEventVersion);

            var snapshotAggregate = snapshotAggregateMock.Object;
            
            var makeSnapshot = itervalSnapshotStrategy.ShouldMakeSnapshot(snapshotAggregate);

            makeSnapshot.Should().Be(expected);
        }

        [Trait(CategoryName, CategoryValue)]
        [Fact]
        public void Should_not_make_snapshot()
        {
            var itervalSnapshotStrategy = new IntervalSnapshotStrategy();

            var aggregateMock = new Mock<IAggregate>();
            var aggregate = aggregateMock.Object;
          
            var makeSnapshot = itervalSnapshotStrategy.ShouldMakeSnapshot(aggregate);

            makeSnapshot.Should().BeFalse();
        }
    }
}