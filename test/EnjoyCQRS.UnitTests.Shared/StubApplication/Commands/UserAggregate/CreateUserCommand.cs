using EnjoyCQRS.Commands;
using System;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.UserAggregate
{
    public class CreateUserCommand : ICommand
    {
        public Guid AggregateId { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime BornDate { get; }

        public CreateUserCommand(Guid aggregateId, string firstName, string lastName, DateTime bornDate)
        {
            AggregateId = aggregateId;
            FirstName = firstName;
            LastName = lastName;
            BornDate = bornDate;
        }
    }
}
