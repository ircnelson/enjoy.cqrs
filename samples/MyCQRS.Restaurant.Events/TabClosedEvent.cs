using System;
using MyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class TabClosedEvent : DomainEvent
    {
        public int TableNumber { get; }
        public decimal AmountPaid { get; }
        public decimal OrdersValue { get; }
        public decimal TipValue { get; }

        public TabClosedEvent(int tableNumber, decimal amountPaid, decimal ordersValue, decimal tipValue)
        {
            TableNumber = tableNumber;
            AmountPaid = amountPaid;
            OrdersValue = ordersValue;
            TipValue = tipValue;
        }
    }
}