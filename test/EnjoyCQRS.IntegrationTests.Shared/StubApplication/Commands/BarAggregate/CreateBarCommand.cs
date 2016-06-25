using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.BarAggregate
{
    public class CreateBarCommand : Command
    {
        public CreateBarCommand(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}