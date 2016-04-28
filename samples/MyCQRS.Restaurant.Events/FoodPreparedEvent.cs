using System;
using System.Collections.Generic;
using EnjoyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class FoodPreparedEvent : DomainEvent
    {
        public IEnumerable<int> MenuNumbers { get; }

        public FoodPreparedEvent(Guid aggregateId, IEnumerable<int> menuNumbers) : base(aggregateId)
        {
            MenuNumbers = menuNumbers;
        }
    }
}