using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate
{
    public class Bar : Aggregate
    {
        public string LastText { get; private set; }

        public Bar()
        {
        }

        private Bar(Guid id)
        {
            Emit(new BarCreated(id));
        }

        public static Bar Create(Guid id)
        {
            return new Bar(id);
        }

        public void Speak(string text)
        {
            Emit(new SpokeSomething(text));
        }

        protected override void RegisterEvents()
        {
            SubscribeTo<BarCreated>(e =>
            {
                Id = e.AggregateId;
            });

            SubscribeTo<SpokeSomething>(e =>
            {
                LastText = e.Text;
            });
        }
    }
}