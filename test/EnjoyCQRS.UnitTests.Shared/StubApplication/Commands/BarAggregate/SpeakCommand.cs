using System;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.BarAggregate
{
    public class SpeakCommand : Command
    {
        public string Text { get; }

        public SpeakCommand(Guid aggregateId, string text) : base(aggregateId)
        {
            Text = text;
        }
    }
}