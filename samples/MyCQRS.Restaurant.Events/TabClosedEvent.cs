using System;
using EnjoyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class TabClosedEvent : DomainEvent
    {
        public int TableNumber { get; }
        public decimal AmountPaid { get; }
        public decimal OrdersValue { get; }
        public decimal TipValue { get; }

        public TabClosedEvent(Guid aggregateId, int tableNumber, decimal amountPaid, decimal ordersValue, decimal tipValue) : base(aggregateId)
        {
            TableNumber = tableNumber;
            AmountPaid = amountPaid;
            OrdersValue = ordersValue;
            TipValue = tipValue;
        }
    }
}