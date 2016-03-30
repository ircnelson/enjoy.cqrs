using System;
using System.Collections.Generic;

namespace MyCQRS.Restaurant.Read.Models
{
    public class TabModel
    {
        public Guid Id { get; set; }

        public int Number { get; set; }

        public string Waiter { get; set; }

        public bool IsOpen { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal OrderedValue { get; set; }

        public decimal TipValue { get; set; }

        public virtual ICollection<OrderItemModel> OrderedItems { get; set; } = new HashSet<OrderItemModel>();

        public TabModel(Guid id, int number, string waiter, bool isOpen = true, decimal amountPaid = 0, decimal orderedValue = 0, decimal tipValue = 0)
        {
            Id = id;
            Number = number;
            Waiter = waiter;
            IsOpen = isOpen;
            AmountPaid = amountPaid;
            OrderedValue = orderedValue;
            TipValue = tipValue;
        }
    }
}