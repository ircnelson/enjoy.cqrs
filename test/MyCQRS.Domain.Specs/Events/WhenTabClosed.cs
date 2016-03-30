using System;
using FluentAssertions;
using Moq;
using MyCQRS.Restaurant.Events;
using MyCQRS.Restaurant.Events.Handlers;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;
using SampleCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Domain.Specs.Events
{
    public class WhenTabClosed : EventTestFixture<TabClosedEvent, TabClosedEventHandler>
    {
        private TabModel _tab;
        
        protected override void SetupDependencies()
        {
            _tab = new TabModel(Guid.NewGuid(), 1, "Nelson");

            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.GetById(It.IsAny<Guid>()))
                .Returns((Guid key) => _tab);

            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.Update(It.IsAny<TabModel>()))
                .Callback<TabModel>((a) => _tab = a);
        }

        protected override TabClosedEvent When()
        {
            return new TabClosedEvent(1, 5, 4.5M, 0.5M);
        }

        [Fact]
        public void Then_the_read_repository_will_be_used_to_update_tab_model()
        {
            OnDependency<IReadRepository<TabModel>>().Verify(x => x.GetById(It.IsAny<Guid>()));
            OnDependency<IReadRepository<TabModel>>().Verify(x => x.Update(It.IsAny<TabModel>()));
        }

        [Fact]
        public void Then_the_tab_will_be_updated_with_expected_informations()
        {
            _tab.Number.Should().Be(1);
            _tab.IsOpen.Should().Be(false);
            _tab.AmountPaid.Should().Be(5);
            _tab.OrderedValue.Should().Be(4.5M);
            _tab.TipValue.Should().Be(.5M);
        }
    }
}