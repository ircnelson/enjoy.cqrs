using System;
using System.Linq;
using FluentAssertions;
using Moq;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;
using MyCQRS.Restaurant.EventsHandlers;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;
using MyCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Restaurant.Tests.Events
{
    public class WhenDrinksOrdered : EventTestFixture<DrinksOrderedEvent, DrinksOrderedEventHandler>
    {
        private TabModel _tab;
        
        protected override void SetupDependencies()
        {
            _tab = new TabModel(Guid.NewGuid(), 1, "Nelson");

            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.GetById(It.IsAny<Guid>()))
                .Returns<Guid>(key => _tab);

            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.Update(It.IsAny<TabModel>()))
                .Callback<TabModel>(model => _tab = model);
        }

        protected override DrinksOrderedEvent When()
        {
            var order1 = new OrderedItem
            {
                MenuNumber = 4,
                Description = "Sprite",
                Price = 1.50M,
                IsDrink = true,
                Status = OrderStatus.Ordered
            };

            return new DrinksOrderedEvent(Guid.NewGuid(), order1.Description, order1.MenuNumber, order1.Price, order1.Status.ToString());
        }

        [Fact]
        public void Then_the_read_repository_should_be_save_ordered_items()
        {
            OnDependency<IReadRepository<TabModel>>().Verify(x => x.GetById(It.IsAny<Guid>()));
            OnDependency<IReadRepository<TabModel>>().Verify(x => x.Update(It.IsAny<TabModel>()));
        }

        [Fact]
        public void Then_should_be_saved_1_items()
        {
            _tab.OrderedItems.Should().HaveCount(1);
        }

        [Fact]
        public void Then_the_order_items_will_be_saved_with_the_expected_details()
        {
            var orders = _tab.OrderedItems.ToList();

            orders[0].MenuNumber.Should().Be(4);
            orders[0].Price.Should().Be(1.5M);
            orders[0].Status.Should().Be(OrderStatus.Ordered.ToString());
        }
    }
}