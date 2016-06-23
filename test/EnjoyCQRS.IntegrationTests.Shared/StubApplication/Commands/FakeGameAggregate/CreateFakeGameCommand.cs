using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FakeGameAggregate
{
    public class CreateFakeGameCommand : Command
    {
        public string PlayerOneName { get; }
        public string PlayerTwoName { get; }

        public CreateFakeGameCommand(Guid aggregateId, string playerOneName, string playerTwoName) : base(aggregateId)
        {
            PlayerOneName = playerOneName;
            PlayerTwoName = playerTwoName;
        }
    }
}