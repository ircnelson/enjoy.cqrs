using System;
using System.Collections.Generic;
using MyCQRS.Events;
using MyCQRS.Restaurant.Commands;
using MyCQRS.Restaurant.Commands.Handlers;
using MyCQRS.Restaurant.Domain;
using MyCQRS.Restaurant.Domain.Exceptions;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;
using MyCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Restaurant.Tests.Commands
{
    public class FoodNotOrderedCanNotBeMarkedPrepared :
        CommandTestFixture<MarkFoodPreparedCommand, MarkFoodPreparedCommandHandler, TabAggregate>
    {
        private Guid _tabAggregate;
        private OrderedItem _testFood1;
        private OrderedItem _testFood2;

        protected override IEnumerable<IDomainEvent> Given()
        {
            _tabAggregate = Guid.NewGuid();

            _testFood1 = new OrderedItem
            {
                MenuNumber = 16,
                Description = "Beef Noodles",
                Price = 7.50M,
                IsDrink = false
            };

            _testFood2 = new OrderedItem
            {
                MenuNumber = 25,
                Description = "Vegetable Curry",
                Price = 6.00M,
                IsDrink = false
            };

            yield return PrepareDomainEvent.Set(new TabOpenedEvent(_tabAggregate, 1, "Nelson")).ToVersion(1);
        }

        protected override MarkFoodPreparedCommand When()
        {
            return new MarkFoodPreparedCommand(_tabAggregate,
                new List<int> {_testFood1.MenuNumber, _testFood2.MenuNumber});
        }

        [Fact]
        public void Then_FoodNotOutstanding_should_be_throws()
        {
            Assert.Equal(typeof (FoodNotOutstanding), CaughtException.GetType());
        }
    }
}