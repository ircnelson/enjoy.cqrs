using System;
using FluentAssertions;
using Moq;
using MyCQRS.Restaurant.Events;
using MyCQRS.Restaurant.EventsHandlers;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;
using MyCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Restaurant.Tests.Events
{
    public class WhenTabOpened : EventTestFixture<TabOpenedEvent, TabOpenedEventHandler>
    {
        private TabModel _tab;

        protected override void SetupDependencies()
        {
            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.Insert(It.IsAny<TabModel>()))
                .Callback<TabModel>(a => _tab = a);
        }

        protected override TabOpenedEvent When()
        {
            return new TabOpenedEvent(Guid.NewGuid(), 1, "Nelson");
        }

        [Fact]
        public void Then_the_read_repository_will_be_used_to_save_tab_model()
        {
            OnDependency<IReadRepository<TabModel>>().Verify(x => x.Insert(It.IsAny<TabModel>()));
        }

        [Fact]
        public void Then_the_tab_will_be_updated_with_expected_informations()
        {
            _tab.Number.Should().Be(1);
            _tab.Waiter.Should().Be("Nelson");

            _tab.OrderedItems.Should().HaveCount(0);

            _tab.AmountPaid.Should().Be(0);
            _tab.OrderedValue.Should().Be(0);
            _tab.TipValue.Should().Be(0);
        }
    }
}