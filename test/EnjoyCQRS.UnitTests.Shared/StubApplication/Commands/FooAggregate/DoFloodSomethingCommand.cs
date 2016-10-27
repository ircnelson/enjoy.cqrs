using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.FooAggregate
{
    public class DoFloodSomethingCommand : Command
    {
        public int Times { get; }

        public DoFloodSomethingCommand(Guid aggregateId, int times) : base(aggregateId)
        {
            Times = times;
        }
    }
}