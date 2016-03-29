using System;

namespace MyCQRS.Commands
{
    public class ChangeNameCommand : Command
    {
        public string Name { get; }

        public ChangeNameCommand(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }
}