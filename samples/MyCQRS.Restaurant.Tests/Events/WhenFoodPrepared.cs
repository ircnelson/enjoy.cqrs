using System;
using System.Collections.Generic;
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
    public class WhenFoodPrepared : EventTestFixture<FoodPreparedEvent, FoodPreparedEventHandler>
    {
        private TabModel _tab;
        private OrderItemModel _food1;
        private OrderItemModel _food2;
        private List<OrderItemModel> _orderItemModels;
        
        protected override void SetupDependencies()
        {   
            _tab = new TabModel(Guid.NewGuid(), 1, "Nelson");

            _food1 = new OrderItemModel(Guid.NewGuid(), _tab.Id, 16, 7.5M, OrderStatus.Ordered.ToString());
            _food2 = new OrderItemModel(Guid.NewGuid(), _tab.Id, 25, 6M, OrderStatus.Ordered.ToString());

            _orderItemModels = new List<OrderItemModel>
            {
                _food1,
                _food2
            };

            _tab.OrderedItems = _orderItemModels;

            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.GetById(It.IsAny<Guid>()))
                .Returns<Guid>(key => _tab);

            OnDependency<IReadRepository<TabModel>>()
                .Setup(x => x.Update(It.IsAny<TabModel>()))
                .Callback<TabModel>(model => _tab = model);
        }

        protected override FoodPreparedEvent When()
        {
            var @event = new FoodPreparedEvent(_tab.Id, new List<int>
            {
                _food2.MenuNumber
            });
            
            return @event;
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
            _orderItemModels.Where(e => e.Status == OrderStatus.Prepared.ToString()).Should().HaveCount(1);
        }

        [Fact]
        public void Then_the_order_items_will_be_saved_with_the_expected_details()
        {
            _orderItemModels[0].Status.Should().Be(OrderStatus.Ordered.ToString());
            _orderItemModels[1].Status.Should().Be(OrderStatus.Prepared.ToString());
        }
    }
}