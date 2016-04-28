using System;
using System.Collections.Generic;
using EnjoyCQRS.Commands;

namespace MyCQRS.Restaurant.Commands
{
    [Serializable]
    public class MarkDrinksServedCommand : Command
    {
        public IEnumerable<int> MenuNumbers { get; }

        public MarkDrinksServedCommand(Guid aggregateId, IEnumerable<int> menuNumbers) : base(aggregateId)
        {
            MenuNumbers = menuNumbers;
        }
    }
}