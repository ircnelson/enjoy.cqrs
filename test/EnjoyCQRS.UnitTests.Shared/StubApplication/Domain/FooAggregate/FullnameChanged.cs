using System;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.FooAggregate
{
    public class FullNameChanged : DomainEvent
    {
        public string FirstName { get; }
        public string LastName { get; }

        public FullNameChanged(Guid aggregateId, string firstName, string lastName) : base(aggregateId)
        {
            LastName = lastName;
            FirstName = firstName;
        }
    }
}