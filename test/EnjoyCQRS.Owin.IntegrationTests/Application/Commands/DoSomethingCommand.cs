using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.Owin.IntegrationTests.Application.Commands
{
    public class DoSomethingCommand : Command
    {
        public DoSomethingCommand(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}