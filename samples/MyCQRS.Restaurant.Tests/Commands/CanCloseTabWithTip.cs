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
using MyCQRS.TestFramework;
using Xunit;

namespace MyCQRS.Restaurant.Tests.Commands
{
    public class CanCloseTabWithTip : CommandTestFixture<CloseTabCommand, CloseTabCommandHandler, TabAggregate>
    {
        private const decimal TipValue = 0.5M;

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
            yield return PrepareDomainEvent.Set(new DrinksServedEvent(_tabAggregate, new List<int> {_testDrink1.MenuNumber})).ToVersion(3);
        }

        protected override CloseTabCommand When()
        {
            return new CloseTabCommand(_tabAggregate, _testDrink1.Price + TipValue);
        }

        [Fact]
        public void Then_tab_closed_event_will_be_published()
        {
            PublishedEvents.Last().Should().BeAssignableTo<TabClosedEvent>();
        }

        [Fact]
        public void Then_tip_should_be_more_than_zero()
        {
            PublishedEvents.Last().As<TabClosedEvent>().TipValue.Should().Be(TipValue);
        }
    }
}