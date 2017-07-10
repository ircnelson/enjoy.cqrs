using EnjoyCQRS.Events;
using System;

namespace EnjoyCQRS.UnitTests.Shared.Projection
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
