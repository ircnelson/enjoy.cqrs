using EnjoyCQRS.Events;
using System;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate
{
    public class UserLastNameChanged : DomainEvent
    {
        public string NewLastname { get; }
        public UserLastNameChanged(Guid aggregateId, string newLastName) : base(aggregateId)
        {
            NewLastname = newLastName;
        }
    }
}
