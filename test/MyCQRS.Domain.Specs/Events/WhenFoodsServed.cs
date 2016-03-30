using System;
using System.Linq;
using FluentAssertions;
using Moq;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;
using MyCQRS.Restaurant.Events.Handlers;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;
using SampleCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Domain.Specs.Events
{
    public class WhenFoodsServed : EventTestFixture<FoodServedEvent, FoodServedEventHandler>
    {
        private TabModel _tab;
        private OrderedItem _food1;
        private OrderedItem _food2;

        protected override void SetupDependencies()
        {
            _tab = new TabModel(Guid.NewGuid(), 1, "Nelson");

            _food1 = new OrderedItem
            {
                MenuNumber = 16,
                Description = "Beef Noodles",
                Price = 7.50M,
                IsDrink = false,
                Status = OrderStatus.Ordered
            };

            _food2 = new OrderedItem
            {
                MenuNumber = 25,
                Description = "Vegetable Curry",
                Price = 6.00M,
                IsDrink = false,
                Status = OrderStatus.Ordered
            };
            
            new[] {_food1, _food2}
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

        protected override FoodServedEvent When()
        {
            return new FoodServedEvent(new []{ _food1.MenuNumber });
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
            _tab.OrderedItems.Should().HaveCount(2);
        }

        [Fact]
        public void Then_should_be_served_items()
        {
            _tab.OrderedItems.Where(e => e.Status == OrderStatus.Served.ToString()).Should().HaveCount(1);
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

            orders[0].MenuNumber.Should().Be(_food1.MenuNumber);
            orders[0].Price.Should().Be(_food1.Price);
            orders[0].Status.Should().Be(OrderStatus.Served.ToString());
            
            orders[1].MenuNumber.Should().Be(_food2.MenuNumber);
            orders[1].Price.Should().Be(_food2.Price);
            orders[1].Status.Should().Be(OrderStatus.Ordered.ToString());
        }
    }
}