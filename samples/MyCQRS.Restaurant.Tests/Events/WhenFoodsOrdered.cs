using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;
using MyCQRS.Restaurant.Events.Handlers;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;
using MyCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Restaurant.Tests.Events
{
    public class WhenFoodsOrdered : EventTestFixture<FoodOrderedEvent, FoodOrderedEventHandler>
    {
        private readonly List<OrderItemModel> _savedOrderItemModels = new List<OrderItemModel>();
        
        protected override void SetupDependencies()
        {
            OnDependency<IReadRepository<OrderItemModel>>()
                .Setup(x => x.Insert(It.IsAny<OrderItemModel>()))
                .Callback<OrderItemModel>(x => _savedOrderItemModels.Add(x));
        }

        protected override FoodOrderedEvent When()
        {
            var order1 = new OrderedItem
            {
                MenuNumber = 16,
                Description = "Beef Noodles",
                Price = 7.50M,
                IsDrink = false
            };

            return new FoodOrderedEvent(Guid.NewGuid(), order1.Description, order1.MenuNumber, order1.Price, order1.Status.ToString());
        }

        [Fact]
        public void Then_the_read_repository_should_be_save_ordered_items()
        {
            OnDependency<IReadRepository<OrderItemModel>>().Verify(x => x.Insert(It.IsAny<OrderItemModel>()));
        }

        [Fact]
        public void Then_should_be_saved_1_items()
        {
            _savedOrderItemModels.Should().HaveCount(1);
        }

        [Fact]
        public void Then_the_order_items_will_be_saved_with_the_expected_details()
        {
            _savedOrderItemModels[0].MenuNumber.Should().Be(16);
            _savedOrderItemModels[0].Price.Should().Be(7.5M);
        }
    }
}