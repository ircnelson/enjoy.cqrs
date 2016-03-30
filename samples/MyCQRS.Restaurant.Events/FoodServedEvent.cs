using System;
using System.Collections.Generic;
using MyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class FoodServedEvent : DomainEvent
    {
        public IEnumerable<int> MenuNumbers { get; }

        public FoodServedEvent(IEnumerable<int> menuNumbers)
        {
            MenuNumbers = menuNumbers;
        }
    }
}