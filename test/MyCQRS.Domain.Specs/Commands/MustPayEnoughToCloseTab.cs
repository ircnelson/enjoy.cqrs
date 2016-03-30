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
    public class MustPayEnoughToCloseTab : CommandTestFixture<CloseTabCommand, CloseTabCommandHandler, TabAggregate>
    {
        private Guid _tabAggregate;
        private OrderedItem _testDrink1;

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

            yield return PrepareDomainEvent.Set(new TabOpenedEvent(_tabAggregate, 1, "Nelson")).ToVersion(1);
            yield return PrepareDomainEvent.Set(new DrinksOrderedEvent(_tabAggregate, _testDrink1.Description, _testDrink1.MenuNumber, _testDrink1.Price, _testDrink1.Status.ToString())).ToVersion(2);
            yield return PrepareDomainEvent.Set(new DrinksServedEvent(new List<int> {_testDrink1.MenuNumber})).ToVersion(3);
        }

        protected override CloseTabCommand When()
        {
            return new CloseTabCommand(_tabAggregate, _testDrink1.Price - 0.5M);
        }

        [Fact]
        public void Then_MustPayEnough_exception_should_be_throws()
        {
            Assert.Equal(typeof (MustPayEnough), CaughtException.GetType());
        }
    }
}