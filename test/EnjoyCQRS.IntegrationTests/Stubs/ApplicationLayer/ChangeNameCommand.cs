using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Stubs.ApplicationLayer
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