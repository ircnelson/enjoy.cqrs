using System;
using System.Collections.Generic;
using MyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class DrinksServedEvent : DomainEvent
    {
        public IEnumerable<int> MenuNumbers { get; }

        public DrinksServedEvent(IEnumerable<int> menuNumbers)
        {
            MenuNumbers = menuNumbers;
        }
    }
}