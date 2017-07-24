using System;
using EnjoyCQRS.Projections;
using EnjoyCQRS.Core;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate.Projections
{
    public class AllUserProjections : IProjector
    {
        private readonly IProjectionWriter<Guid, AllUserView> _store;

        public AllUserProjections(IProjectionStore store)
        {
            _store = store.GetWriter<Guid, AllUserView>();
        }

        public void When(Metadata<UserCreated> e)
        {
            _store.Add(e.Data.AggregateId, new AllUserView
            {
                Id = e.Data.AggregateId,
                Fullname = $"{e.Data.LastName}, {e.Data.FirstName}",
                BirthMonth = e.Data.BornDate.Month,
                BirthYear = e.Data.BornDate.Year,
                CreatedAt = e.Data.CreatedAt
            });
        }

        public void When(UserFirstNameChanged value)
        {
            _store.UpdateOrThrow(value.AggregateId, (view) =>
            {
                var lname = view.Fullname.Split(',')[0].Trim();

                view.Fullname = $"{lname}, {value.NewFirstName}";
            });
        }

        public void When(UserLastNameChanged value)
        {
            _store.UpdateOrThrow(value.AggregateId, (view) =>
            {
                var fname = view.Fullname.Split(',')[1].Trim();

                view.Fullname = $"{value.NewLastname}, {fname}";
            });
        }

        public void When(UserDeactivated e)
        {
            _store.UpdateOrThrow(e.AggregateId, (view) =>
            {
                view.DeactivatedAt = e.DeactivatedAt;
                view.Lifetime = view.CreatedAt.Subtract(e.DeactivatedAt).Duration();
            });
        }
    }
}
