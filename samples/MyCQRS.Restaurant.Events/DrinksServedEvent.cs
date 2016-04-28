using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class DrinksServedEvent : DomainEvent
    {
        public IEnumerable<int> MenuNumbers { get; }

        public DrinksServedEvent(Guid aggregateId, IEnumerable<int> menuNumbers) : base(aggregateId)
        {
            MenuNumbers = menuNumbers;
        }
    }
}