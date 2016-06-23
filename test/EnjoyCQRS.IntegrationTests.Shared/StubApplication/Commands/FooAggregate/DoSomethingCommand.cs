using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FooAggregate
{
    public class DoSomethingCommand : Command
    {
        public DoSomethingCommand(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}