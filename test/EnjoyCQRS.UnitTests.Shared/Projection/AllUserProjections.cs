using System;
using EnjoyCQRS.Projections;

namespace EnjoyCQRS.UnitTests.Shared.Projection
{
    public class AllUserProjections : Projector<Guid, AllUserView>
    {
        public AllUserProjections(IProjectionWriter<Guid, AllUserView> store) : base(store)
        {
        }

        public void When(UserCreated e)
        {
            Store.Add(e.AggregateId, new AllUserView
            {
                Id = e.AggregateId,
                Fullname = $"{e.LastName}, {e.FirstName}",
                BirthMonth = e.BornDate.Month,
                BirthYear = e.BornDate.Year,
                CreatedAt = e.CreatedAt
            });
        }

        public void When(UserFirstNameChanged value)
        {
            Store.UpdateOrThrow(value.AggregateId, (view) =>
            {
                var lname = view.Fullname.Split(',')[0].Trim();

                view.Fullname = $"{lname}, {value.NewFirstName}";
            });
        }

        public void When(UserLastNameChanged value)
        {
            Store.UpdateOrThrow(value.AggregateId, (view) =>
            {
                var fname = view.Fullname.Split(',')[1].Trim();

                view.Fullname = $"{value.NewLastname}, {fname}";
            });
        }

        public void When(UserDeactivated e)
        {
            Store.UpdateOrThrow(e.AggregateId, (view) =>
            {
                view.DeactivatedAt = e.DeactivatedAt;
                view.Lifetime = view.CreatedAt.Subtract(e.DeactivatedAt).Duration();
            });
        }
    }
}
