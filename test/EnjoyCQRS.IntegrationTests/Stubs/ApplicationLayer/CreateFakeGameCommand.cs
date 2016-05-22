using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Stubs.ApplicationLayer
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