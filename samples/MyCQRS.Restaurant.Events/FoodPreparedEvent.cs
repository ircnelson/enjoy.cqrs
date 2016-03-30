using System;
using System.Collections.Generic;
using MyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class FoodPreparedEvent : DomainEvent
    {
        public IEnumerable<int> MenuNumbers { get; }

        public FoodPreparedEvent(IEnumerable<int> menuNumbers)
        {
            MenuNumbers = menuNumbers;
        }
    }
}