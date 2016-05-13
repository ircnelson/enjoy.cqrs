using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Bus.InProcess
{
    public class EventBus : InProcessBus, IEventPublisher
    {
        private readonly IEventRouter _router;

        public EventBus(IEventRouter router)
        {
            _router = router;
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
        
        protected override async Task RouteAsync(dynamic message)
        {
            await _router.RouteAsync(message);
        }
    }
}