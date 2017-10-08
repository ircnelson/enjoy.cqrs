using System;
using EnjoyCQRS.Projections;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate.Projections
{
    public class OnlyActiveUserProjections : IProjector
    {
        private readonly IProjectionWriter<Guid, ActiveUserView> _store;

        public OnlyActiveUserProjections(IProjectionStore store)
        {
            _store = store.GetWriter<Guid, ActiveUserView>();
        }

        public void When(UserCreated e)
        {
            _store.Add(e.AggregateId, new ActiveUserView
            {
                Id = e.AggregateId
            });
        }

        public void When(UserDeactivated e)
        {
            _store.TryDelete(e.AggregateId);
        }
    }

    public interface IProjector
    {

    }
}
