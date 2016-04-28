using System;
using System.Collections.Generic;
using EnjoyCQRS.TestFramework;
using MyCQRS.Restaurant.Commands;
using MyCQRS.Restaurant.CommandsHandlers;
using MyCQRS.Restaurant.Domain;
using MyCQRS.Restaurant.Domain.Exceptions;
using MyCQRS.Restaurant.Domain.ValueObjects;
using Xunit;

namespace MyCQRS.Restaurant.Tests.Commands
{
    public class CanNotOrderWithUnopenedTab :
        CommandTestFixture<PlaceOrderCommand, PlaceOrderCommandHandler, TabAggregate>
    {
        protected override PlaceOrderCommand When()
        {
            var testDrink1 = new OrderedItem
            {
                MenuNumber = 4,
                Description = "Sprite",
                Price = 1.50M,
                IsDrink = true
            };

            return new PlaceOrderCommand(Guid.NewGuid(), new List<OrderedItem> {testDrink1});
        }

        [Fact]
        public void Then_TabNotOpenException_should_be_throws()
        {
            Assert.Equal(typeof (TabNotOpen), CaughtException.GetType());
        }
    }
}