using EnjoyCQRS.Events;
using System;

namespace EnjoyCQRS.UnitTests.Shared.Projection
{
    public class UserCreated : DomainEvent
    {
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime BornDate { get; }
        public DateTime CreatedAt { get; }

        public UserCreated(Guid aggregateId, DateTime createdAt, string firstName, string lastName, DateTime bornDate) : base(aggregateId)
        {
            CreatedAt = createdAt;
            FirstName = firstName;
            LastName = lastName;
            BornDate = bornDate;
        }
    }
}
