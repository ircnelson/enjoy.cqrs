using System;
using System.Linq;
using EnjoyCQRS.TestFramework;
using FluentAssertions;
using Moq;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;
using MyCQRS.Restaurant.EventsHandlers;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;
using Xunit;

namespace MyCQRS.Restaurant.Tests.Events
{
    public class WhenDrinksServed : EventTestFixture<DrinksServedEvent, DrinksServedEventHandler>
    {
        private TabModel _tab;
        private OrderedItem _drink1;
        private OrderedItem _drink2;
        private OrderedItem _drink3;

        protected override void SetupDependencies()
        {
            _tab = new TabModel(Guid.NewGuid(), 1, "Nelson");
            
            _drink1 = new OrderedItem
            {
                MenuNumber = 4,
                Description = "Sprite",
                Price = 1.50M,
                IsDrink = true,
                Status = OrderStatus.Ordered
            };

            _drink2 = new OrderedItem
            {
                MenuNumber = 10,
                Description = "Beer",
                Price = 2.50M,
                IsDrink = true,
                Status = OrderStatus.Ordered
            };

            _drink3 = new OrderedItem
            {
                MenuNumber = 12,
                Description = "Coca",
                Price = 3.50M,
                IsDrink = true,
                Status = OrderStatus.Ordered
            };

            new[] {_drink1, _drink2, _drink3}
                .ToList()
                .ForEach(e =>
                {
                    _tab.OrderedItems.Add(new OrderItemModel(Guid.NewGuid(), _tab.Id, e.MenuNumber, e.Price, e.Status.ToString()));
                });
            
            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.GetById(It.IsAny<Guid>()))
                .Returns<Guid>(key => _tab);

            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.Update(It.IsAny<TabModel>()))
                .Callback<TabModel>(model => _tab = model);
        }

        protected override DrinksServedEvent When()
        {
            return new DrinksServedEvent(Guid.NewGuid(), new []{ _drink1.MenuNumber, _drink2.MenuNumber });
        }

        [Fact]
        public void Then_the_read_repository_should_be_save_ordered_items()
        {
            OnDependency<IReadRepository<TabModel>>().Verify(x => x.GetById(It.IsAny<Guid>()));
            OnDependency<IReadRepository<TabModel>>().Verify(x => x.Update(It.IsAny<TabModel>()));
        }

        [Fact]
        public void Then_should_be_ordered_items()
        {
            _tab.OrderedItems.Should().HaveCount(3);
        }

        [Fact]
        public void Then_should_be_served_items()
        {
            _tab.OrderedItems.Where(e => e.Status == OrderStatus.Served.ToString()).Should().HaveCount(2);
        }

        [Fact]
        public void Then_should_be_calculate_orderedValue_only_served_items()
        {
            var servedOrders = _tab.OrderedItems.Where(o => o.Status == OrderStatus.Served.ToString());
            _tab.OrderedValue.Should().Be(servedOrders.Sum(o => o.Price));
        }

        [Fact]
        public void Then_the_order_items_will_be_saved_with_the_expected_details()
        {
            var orders = _tab.OrderedItems.ToList();

            orders[0].MenuNumber.Should().Be(_drink1.MenuNumber);
            orders[0].Price.Should().Be(_drink1.Price);
            orders[0].Status.Should().Be(OrderStatus.Served.ToString());

            orders[1].MenuNumber.Should().Be(_drink2.MenuNumber);
            orders[1].Price.Should().Be(_drink2.Price);
            orders[1].Status.Should().Be(OrderStatus.Served.ToString());

            orders[2].MenuNumber.Should().Be(_drink3.MenuNumber);
            orders[2].Price.Should().Be(_drink3.Price);
            orders[2].Status.Should().Be(OrderStatus.Ordered.ToString());
        }
    }
}