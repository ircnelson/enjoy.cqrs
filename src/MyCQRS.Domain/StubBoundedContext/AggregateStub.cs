using MyCQRS.Domain.StubBoundedContext.Events;

namespace MyCQRS.Domain.StubBoundedContext
{
    public class AggregateStub : Aggregate
    {
        public string Name { get; private set; }
        public bool IsActive { get; set; }

        public AggregateStub()
        {
            On<ChangedNameEvent>(Apply);
            On<ActivatedEvent>(Apply);
        }

        private void Apply(ActivatedEvent e)
        {
            IsActive = true;
        }

        private void Apply(ChangedNameEvent e)
        {
            Name = e.Name;
        }

        public void ChangeName(string newName)
        {
            Raise(new ChangedNameEvent(newName));
        }

        public void Activate()
        {
            Raise(new ActivatedEvent());
        }

        public void Deactivate()
        {
            Raise(new DeactivatedEvent());
        }
    }
}