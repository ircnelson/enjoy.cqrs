using EnjoyCQRS.Events;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.BarAggregate
{
    public class SpokeSomething : IDomainEvent
    {
        public string Text { get; }

        public SpokeSomething(string text)
        {
            Text = text;
        }
    }
}