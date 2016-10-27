using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.FooAggregate
{
    public class DoSomethingCommand : Command
    {
        public DoSomethingCommand(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}