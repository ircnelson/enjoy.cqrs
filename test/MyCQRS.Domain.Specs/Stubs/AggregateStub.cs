namespace MyCQRS.Domain.Specs.Stubs
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
    }
}