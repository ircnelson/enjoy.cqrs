using EnjoyCQRS.Commands;
using System;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.UserAggregate
{
    public class ChangeFirstNameCommand : ICommand
    {
        public Guid AggregateId { get; }
        public string Name { get; }

        public ChangeFirstNameCommand(Guid aggregateId, string name)
        {
            AggregateId = aggregateId;
            Name = name;
        }
    }
}
