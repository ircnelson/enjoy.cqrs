using EnjoyCQRS.Events;
using System;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate
{
    public class UserFirstNameChanged : DomainEvent
    {
        public string NewFirstName { get; }
        public UserFirstNameChanged(Guid aggregateId, string newFirstName) : base(aggregateId)
        {
            NewFirstName = newFirstName;
        }
    }
}
