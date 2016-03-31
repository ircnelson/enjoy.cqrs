using System;
using System.Collections.Generic;
using System.Linq;
using MyCQRS.EventStore;
using MyCQRS.Restaurant.Domain.Exceptions;
using MyCQRS.Restaurant.Domain.ValueObjects;
using MyCQRS.Restaurant.Events;

namespace MyCQRS.Restaurant.Domain
{
    public class TabAggregate : Aggregate
    {
        private readonly List<OrderedItem> _outstandingDrinks = new List<OrderedItem>();
        private readonly List<OrderedItem> _outstandingFood = new List<OrderedItem>();
        private readonly List<OrderedItem> _preparedFood = new List<OrderedItem>();
        private bool _open;
        private int _tableNumber;
        private decimal _servedItemsValue;

        public TabAggregate()
        {
            RegisterEvents();
        }

        private TabAggregate(int tableNumber, string waiter) : this()
        {
            Raise(new TabOpenedEvent(Guid.NewGuid(), tableNumber, waiter));
        }
        public static TabAggregate Create(int tableNumber, string waiter)
        {
            return new TabAggregate(tableNumber, waiter);
        }
        
        public void PlaceOrder(IEnumerable<OrderedItem> orderedItems)
        {
            if (!_open)
                throw new TabNotOpen();
            
            var drinks = orderedItems.Where(i => i.IsDrink).ToList();
            if (drinks.Any())
            {
                foreach (var orderedItem in drinks)
                {
                    Raise(new DrinksOrderedEvent(Id, orderedItem.Description, orderedItem.MenuNumber, orderedItem.Price, orderedItem.Status.ToString()));
                }

            }

            var foods = orderedItems.Where(i => !i.IsDrink).ToList();
            if (foods.Any())
            {
                foreach (var orderedItem in foods)
                {                   
                    Raise(new FoodOrderedEvent(Id, orderedItem.Description, orderedItem.MenuNumber, orderedItem.Price, orderedItem.Status.ToString()));
                }
            }
        }
        public void MarkDrinksServed(IEnumerable<int> menuNumbers)
        {
            if (!AreDrinksOutstanding(menuNumbers))
                throw new DrinksNotOutstanding();

            Raise(new DrinksServedEvent(Id, menuNumbers));
        }

        public void MarkFoodPrepared(IEnumerable<int> menuNumbers)
        {
            if (!IsFoodOutstanding(menuNumbers))
                throw new FoodNotOutstanding();

            Raise(new FoodPreparedEvent(Id, menuNumbers));
        }

        public void MarkFoodServed(IEnumerable<int> menuNumbers)
        {
            if (!IsFoodPrepared(menuNumbers))
                throw new FoodNotPrepared();

            Raise(new FoodServedEvent(Id, menuNumbers));
        }

        public void CloseTab(decimal amountPaid)
        {
            if (!_open)
                throw new TabNotOpen();
            if (HasUnservedItems())
                throw new TabHasUnservedItems();
            if (amountPaid < _servedItemsValue)
                throw new MustPayEnough();

            var ordersValue = _servedItemsValue;
            var tipValue = amountPaid - _servedItemsValue;

            Raise(new TabClosedEvent(Id, _tableNumber, amountPaid, ordersValue, tipValue));
        }

        private bool HasUnservedItems()
        {
            return _outstandingDrinks.Any() || _outstandingFood.Any() || _preparedFood.Any();
        }

        private bool IsFoodPrepared(IEnumerable<int> menuNumbers)
        {
            return AreAllInList(want: menuNumbers, have: _preparedFood);
        }

        private bool IsFoodOutstanding(IEnumerable<int> menuNumbers)
        {
            return AreAllInList(want: menuNumbers, have: _outstandingFood);
        }

        private bool AreDrinksOutstanding(IEnumerable<int> menuNumbers)
        {
            return AreAllInList(want: menuNumbers, have: _outstandingDrinks);
        }

        private bool AreAllInList(IEnumerable<int> want, IEnumerable<OrderedItem> have)
        {
            var curHave = new List<int>(have.Select(i => i.MenuNumber));
            foreach (var num in want)
                if (curHave.Contains(num))
                    curHave.Remove(num);
                else
                    return false;
            return true;
        }

        private void RegisterEvents()
        {
            On<TabOpenedEvent>(OnNewTabOpened);
            On<DrinksOrderedEvent>(OnNewDrinksOrdered);
            On<FoodOrderedEvent>(OnNewFoodsOrdered);
            On<DrinksServedEvent>(OnNewDrinksServed);
            On<FoodPreparedEvent>(OnNewFoodPrepared);
            On<FoodServedEvent>(OnNewFoodServed);
            On<TabClosedEvent>(OnTabClosed);
        }

        private void OnTabClosed(TabClosedEvent obj)
        {
            _open = false;
        }

        private void OnNewFoodServed(FoodServedEvent evt)
        {
            foreach (var num in evt.MenuNumbers)
            {
                var item = _preparedFood.First(d => d.MenuNumber == num);
                _preparedFood.Remove(item);
                _servedItemsValue += item.Price;
            }
        }

        private void OnNewFoodPrepared(FoodPreparedEvent evt)
        {
            foreach (var num in evt.MenuNumbers)
            {
                var item = _outstandingFood.First(f => f.MenuNumber == num);
                _outstandingFood.Remove(item);
                _preparedFood.Add(item);
            }
        }

        private void OnNewDrinksServed(DrinksServedEvent evt)
        {
            foreach (var num in evt.MenuNumbers)
            {
                var item = _outstandingDrinks.First(d => d.MenuNumber == num);
                _outstandingDrinks.Remove(item);
                _servedItemsValue += item.Price;
            }
        }

        private void OnNewFoodsOrdered(FoodOrderedEvent evt)
        {
            _outstandingFood.Add(new OrderedItem
            {
                MenuNumber = evt.MenuNumber,
                Status = (OrderStatus)Enum.Parse(typeof(OrderStatus), evt.Status),
                Price = evt.Price,
                Description = evt.Description,
                IsDrink = false
            });
        }

        private void OnNewDrinksOrdered(DrinksOrderedEvent evt)
        {
            _outstandingDrinks.Add(new OrderedItem
            {
                MenuNumber = evt.MenuNumber,
                Status = (OrderStatus) Enum.Parse(typeof(OrderStatus), evt.Status),
                Price = evt.Price,
                Description = evt.Description,
                IsDrink = true
            });
        }

        private void OnNewTabOpened(TabOpenedEvent evt)
        {
            Id = evt.AggregateId;
            _tableNumber = evt.TableNumber;
            _open = true;
        }

    }
}