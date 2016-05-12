using System.Collections.Generic;
using EnjoyCQRS.Events;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Bus.InProcess
{
    public class EventBus : InProcessBus<IDomainEvent>, IEventPublisher
    {
        private readonly IEventRouter _router;

        public EventBus(IEventRouter router)
        {
            _router = router;
        }

        protected override void Route(IDomainEvent message)
        {
            _router.Route(message);
        }

        public void Publish<TEvent>(TEvent message) where TEvent : IDomainEvent
        {
            Send(message);
        }

        public void Publish<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent
        {
            foreach (var message in messages)
            {
                Send(message);
            }
        }
    }
}