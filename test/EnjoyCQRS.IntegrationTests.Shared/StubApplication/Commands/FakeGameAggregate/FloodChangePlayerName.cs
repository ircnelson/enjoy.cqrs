using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FakeGameAggregate
{
    public class FloodChangePlayerName : Command
    {
        public int Player { get; }
        public string Name { get; }
        public int Times { get; }

        public FloodChangePlayerName(Guid aggregateId, int player, string name, int times) : base(aggregateId)
        {
            Player = player;
            Name = name;
            Times = times;
        }
    }
}