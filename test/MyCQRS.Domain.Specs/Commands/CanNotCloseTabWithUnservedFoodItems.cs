using System;
using System.Collections.Generic;
using MyCQRS.Events;
using MyCQRS.Restaurant.Commands;
using MyCQRS.Restaurant.Commands.Handlers;
using MyCQRS.Restaurant.Domain;
using MyCQRS.Restaurant.Domain.Exceptions;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;
using SampleCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Domain.Specs.Commands
{
    public class CanNotCloseTabWithUnservedFoodItems :
        CommandTestFixture<CloseTabCommand, CloseTabCommandHandler, TabAggregate>
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
            yield return PrepareDomainEvent.Set(new FoodOrderedEvent(_tabAggregate, _testFood1.Description, _testFood1.MenuNumber, _testFood1.Price, _testFood1.Status.ToString())).ToVersion(2);
            yield return PrepareDomainEvent.Set(new FoodOrderedEvent(_tabAggregate, _testFood2.Description, _testFood2.MenuNumber, _testFood2.Price, _testFood2.Status.ToString())).ToVersion(3);
            yield return PrepareDomainEvent.Set(new FoodPreparedEvent(new List<int> {_testFood1.MenuNumber, _testFood2.MenuNumber})).ToVersion(4);
        }

        protected override CloseTabCommand When()
        {
            return new CloseTabCommand(_tabAggregate, _testFood1.Price + _testFood2.Price);
        }

        [Fact]
        public void Then_TabHasUnservedItems_exception_should_be_throw()
        {
            Assert.Equal(typeof (TabHasUnservedItems), CaughtException.GetType());
        }
    }
}