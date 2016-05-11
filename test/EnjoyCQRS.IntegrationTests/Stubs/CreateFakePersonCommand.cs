using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class CreateFakePersonCommand : Command
    {
        public string Name { get; }

        public CreateFakePersonCommand(Guid aggregateId, string name) : base(aggregateId)
        {
            Name = name;
        }
    }
}