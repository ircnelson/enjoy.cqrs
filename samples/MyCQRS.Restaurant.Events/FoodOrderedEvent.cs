using System;
using EnjoyCQRS.Events;

namespace MyCQRS.Restaurant.Events
{
    [Serializable]
    public class FoodOrderedEvent : DomainEvent
    {
        public string Description { get; }
        public int MenuNumber { get; }
        public decimal Price { get; }
        public string Status { get; }

        public FoodOrderedEvent(Guid aggregateId, string description, int menuNumber, decimal price, string status) : base(aggregateId)
        {
            Description = description;
            MenuNumber = menuNumber;
            Price = price;
            Status = status;
        }
    }
}