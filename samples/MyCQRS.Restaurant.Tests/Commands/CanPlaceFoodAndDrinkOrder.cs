using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MyCQRS.Events;
using MyCQRS.Restaurant.Commands;
using MyCQRS.Restaurant.CommandsHandlers;
using MyCQRS.Restaurant.Domain;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;
using MyCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Restaurant.Tests.Commands
{
    public class CanPlaceFoodAndDrinkOrder :
        CommandTestFixture<PlaceOrderCommand, PlaceOrderCommandHandler, TabAggregate>
    {
        private Guid _tabAggregate;
        private OrderedItem _testDrink1;
        private OrderedItem _testFood1;

        protected override IEnumerable<IDomainEvent> Given()
        {
            _tabAggregate = Guid.NewGuid();

            _testDrink1 = new OrderedItem
            {
                MenuNumber = 4,
                Description = "Sprite",
                Price = 1.50M,
                IsDrink = true
            };

            _testFood1 = new OrderedItem
            {
                MenuNumber = 16,
                Description = "Beef Noodles",
                Price = 7.50M,
                IsDrink = false
            };

            yield return PrepareDomainEvent.Set(new TabOpenedEvent(_tabAggregate, 1, "Nelson")).ToVersion(1);
        }

        protected override PlaceOrderCommand When()
        {
            return new PlaceOrderCommand(_tabAggregate, new List<OrderedItem> {_testDrink1, _testFood1});
        }

        [Fact]
        public void Then_drinks_and_foods_ordered_event_will_be_published()
        {
            PublishedEvents.First().Should().BeAssignableTo<DrinksOrderedEvent>();
            PublishedEvents.Last().Should().BeAssignableTo<FoodOrderedEvent>();
        }
    }
}