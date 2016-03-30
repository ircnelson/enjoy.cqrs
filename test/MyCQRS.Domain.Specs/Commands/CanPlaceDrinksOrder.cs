using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MyCQRS.Events;
using MyCQRS.Restaurant.Commands;
using MyCQRS.Restaurant.Commands.Handlers;
using MyCQRS.Restaurant.Domain;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;
using SampleCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Domain.Specs.Commands
{
    public class CanPlaceDrinksOrder : CommandTestFixture<PlaceOrderCommand, PlaceOrderCommandHandler, TabAggregate>
    {
        private Guid _tabAggregate;
        private OrderedItem _testDrink1;
        private OrderedItem _testDrink2;

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

            _testDrink2 = new OrderedItem
            {
                MenuNumber = 10,
                Description = "Beer",
                Price = 2.50M,
                IsDrink = true
            };

            yield return PrepareDomainEvent.Set(new TabOpenedEvent(_tabAggregate, 1, "Nelson")).ToVersion(1);
        }

        protected override PlaceOrderCommand When()
        {
            return new PlaceOrderCommand(_tabAggregate, new List<OrderedItem> {_testDrink1, _testDrink2});
        }

        [Fact]
        public void Then_drinks_ordered_event_will_be_published()
        {
            PublishedEvents.Last().Should().BeAssignableTo<DrinksOrderedEvent>();
        }
    }
}