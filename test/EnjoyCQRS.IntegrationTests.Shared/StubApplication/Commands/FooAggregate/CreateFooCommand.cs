using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FooAggregate
{
    public class CreateFooCommand : Command
    {
        public CreateFooCommand(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}