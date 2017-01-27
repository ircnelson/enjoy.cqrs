using EnjoyCQRS.EventSource.Projections;
using FluentAssertions;
using System.Linq;
using Xunit;
using System;
using System.Reflection;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate.Projections;
using System.Threading.Tasks;

namespace EnjoyCQRS.UnitTests.Projections
{
    public class ProjectionAttributeTests
    {
        [Fact]
        public async Task Should_scan_all_attributes_for_aggregate()
        {
            var bar = Bar.Create(Guid.NewGuid());

            var scanner = new ProjectionProviderAttributeScanner();

            var result = await scanner.ScanAsync(bar.GetType()).ConfigureAwait(false);

            result.Providers.Count().Should().Be(3);
        }

        [Fact]
        public void Should_throw_exception_when_target_is_not_aggregate()
        {
            var scanner = new ProjectionProviderAttributeScanner();

            Func<Task> func = async () => await scanner.ScanAsync(typeof(FakeNonAggregate)).ConfigureAwait(false);

            func.ShouldThrowExactly<TargetException>();
        }

        [Fact]
        public async Task Should_provide_a_projection_instance_from_aggregate()
        {
            var bar = Bar.Create(Guid.NewGuid());

            var scanner = new ProjectionProviderAttributeScanner();

            var result = await scanner.ScanAsync(bar.GetType()).ConfigureAwait(false);

            var provider = result.Providers.First(e => e.GetType().Name == nameof(BarProjectionProvider));

            var projection = (BarProjection) provider.CreateProjection(bar);

            projection.Id.Should().Be(bar.Id);
            projection.LastText.Should().Be(bar.LastText);
            projection.Messages.Count.Should().Be(bar.Messages.Count);
            projection.UpdatedAt.Should().Be(bar.UpdatedAt);
        }
    }
    
    internal class FakeNonAggregate
    {

    }
}
