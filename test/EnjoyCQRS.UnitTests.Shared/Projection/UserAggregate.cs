using EnjoyCQRS.EventSource;
using System;

namespace EnjoyCQRS.UnitTests.Shared.Projection
{
    public class UserAggregate : Aggregate
    {
        public DateTime CreatedAt { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public DateTime BornDate { get; private set; }
        public DateTime? DeactivatedAt { get; private set; }

        public UserAggregate(Guid id, string firstName, string lastName, DateTime bornDate)
        {
            Emit(new UserCreated(id, DateTime.Now, firstName, lastName, bornDate));
        }

        public void ChangeFirstName(string newFirstName)
        {
            Emit(new UserFirstNameChanged(Id, newFirstName));
        }

        public void ChangeLastName(string newLastName)
        {
            Emit(new UserLastNameChanged(Id, newLastName));
        }

        public void Deactivate()
        {
            Emit(new UserDeactivated(Id, DateTime.Now));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<UserCreated>(Apply);
            SubscribeTo<UserFirstNameChanged>(Apply);
            SubscribeTo<UserLastNameChanged>(Apply);
            SubscribeTo<UserDeactivated>(Apply);
        }
        
        private void Apply(UserCreated e)
        {
            Id = e.AggregateId;
            CreatedAt = e.CreatedAt;
            FirstName = e.FirstName;
            LastName = e.LastName;
            BornDate = e.BornDate;
        }

        private void Apply(UserFirstNameChanged e)
        {
            FirstName = e.NewFirstName;
        }

        private void Apply(UserLastNameChanged e)
        {
            LastName = e.NewLastname;
        }

        private void Apply(UserDeactivated e)
        {
            DeactivatedAt = e.DeactivatedAt;
        }
    }
}
