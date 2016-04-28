using System;
using System.Collections.Generic;
using EnjoyCQRS.Commands;
using MyCQRS.Restaurant.Domain.ValueObjects;

namespace MyCQRS.Restaurant.Commands
{
    [Serializable]
    public class PlaceOrderCommand : Command
    {
        public IEnumerable<OrderedItem> OrderedItems { get; }

        public PlaceOrderCommand(Guid aggregateId, IEnumerable<OrderedItem> orderedItems) : base(aggregateId)
        {
            OrderedItems = orderedItems;
        }
    }
}