using System;

namespace MyCQRS.Restaurant.Read.Models
{
    public class OrderItemModel
    {
        public Guid Id { get; set; }
        public Guid TabModelId { get; set; }
        public int MenuNumber { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }

        public OrderItemModel(Guid id, Guid tabModelId, int menuNumber, decimal price, string status)
        {
            Id = id;
            TabModelId = tabModelId;
            MenuNumber = menuNumber;
            Price = price;
            Status = status;
        }
    }
}