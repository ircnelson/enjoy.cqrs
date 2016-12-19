using System;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.TestFramework;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.UnitTests.Handlers
{
    public class EventHandlerTests : EventTestFixture<EventHandlerTests.StubCreatedEvent, EventHandlerTests.StubCreatedEventHandler>
    {
        private const string CategoryName = "Unit";
        private const string CategoryValue = "Handlers";

        private Guid _id;
        
        protected override StubCreatedEvent When()
        {
            _id = Guid.NewGuid();

            return new StubCreatedEvent(_id);
        }

        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Executed_property_should_be_true()
        {
            EventHandler.Executed.Should().Be(true);
        }


        [Trait(CategoryName, CategoryValue)]
        [Then]
        public void Should_pass_the_correct_AggregateId()
        {
            EventHandler.AggregateId.Should().Be(_id);
        }
        
        public class StubCreatedEvent : DomainEvent
        {
            public StubCreatedEvent(Guid aggregateId) : base(aggregateId)
            {
            }
        }

        public class StubCreatedEventHandler : IEventHandler<StubCreatedEvent>
        {
            public Guid AggregateId { get; private set; }
            public bool Executed { get; private set; }

            public Task ExecuteAsync(StubCreatedEvent @event)
            {
                AggregateId = @event.AggregateId;

                Executed = true;

                return Task.CompletedTask;
            }
        }
    }
}