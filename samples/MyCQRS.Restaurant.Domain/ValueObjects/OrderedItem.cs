namespace MyCQRS.Restaurant.Domain.ValueObjects
{
    public struct OrderedItem
    {
        public int MenuNumber { get; set; }
        public string Description { get; set; }
        public OrderStatus Status { get; set; }
        public bool IsDrink { get; set; }
        public decimal Price { get; set; }
    }

    public enum OrderStatus
    {
        Ordered,
        Prepared,
        Served
    }
}