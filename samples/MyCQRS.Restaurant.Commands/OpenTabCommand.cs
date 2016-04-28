using System;
using EnjoyCQRS.Commands;

namespace MyCQRS.Restaurant.Commands
{
    [Serializable]
    public class OpenTabCommand : Command
    {
        public OpenTabCommand(Guid aggregateId, int tableNumber, string waiter) : base(aggregateId)
        {
            TableNumber = tableNumber;
            Waiter = waiter;
        }
        
        public int TableNumber { get; private set; }
        public string Waiter { get; private set; }
    }
}