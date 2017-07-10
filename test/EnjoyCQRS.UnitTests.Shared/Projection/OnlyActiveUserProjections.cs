using System;
using EnjoyCQRS.Projections;

namespace EnjoyCQRS.UnitTests.Shared.Projection
{
    public class OnlyActiveUserProjections : Projector<Guid, ActiveUserView>
    {
        public OnlyActiveUserProjections(IDocumentWriter<Guid, ActiveUserView> store) : base(store)
        {
        }

        public void When(UserCreated e)
        {
            Store.Add(e.AggregateId, new ActiveUserView
            {
                Id = e.AggregateId
            });
        }

        public void When(UserDeactivated e)
        {
            Store.TryDelete(e.AggregateId);
        }
    }
}
