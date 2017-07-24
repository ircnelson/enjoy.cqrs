using EnjoyCQRS.Events;
using System;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate
{
    public class UserDeactivated : DomainEvent
    {
        public DateTime DeactivatedAt { get; }

        public UserDeactivated(Guid aggregateId, DateTime deactivatedAt) : base(aggregateId)
        {
            DeactivatedAt = deactivatedAt;
        }
    }
}
