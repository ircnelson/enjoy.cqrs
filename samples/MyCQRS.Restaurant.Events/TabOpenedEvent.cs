using System;
using MyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class TabOpenedEvent : DomainEvent
    {
        public Guid TabId { get; set; }
        public int TableNumber { get; }
        public string Waiter { get; }

        public TabOpenedEvent(Guid tabId, int tableNumber, string waiter)
        {
            TabId = tabId;
            TableNumber = tableNumber;
            Waiter = waiter;
        }
    }
}