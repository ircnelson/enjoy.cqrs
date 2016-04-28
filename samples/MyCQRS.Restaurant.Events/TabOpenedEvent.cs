using System;
using EnjoyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class TabOpenedEvent : DomainEvent
    {
        public int TableNumber { get; }
        public string Waiter { get; }

        public TabOpenedEvent(Guid aggregateId, int tableNumber, string waiter) : base(aggregateId)
        {
            TableNumber = tableNumber;
            Waiter = waiter;
        }
    }
}