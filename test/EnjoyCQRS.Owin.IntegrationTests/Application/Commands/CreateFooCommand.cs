using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.Owin.IntegrationTests.Application.Commands
{
    public class CreateFooCommand : Command
    {
        public CreateFooCommand(Guid aggregateId) : base(aggregateId)
        {
        }
    }
}