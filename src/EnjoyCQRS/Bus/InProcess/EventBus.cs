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
        
        public async Task PublishAsync<TEvent>(TEvent message) where TEvent : IDomainEvent
        {
            await SendAsync(message);
        }

        public async Task PublishAsync<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent
        {
            foreach (var message in messages)
            {
                await SendAsync(message);
            }
        }
        
        protected override async Task RouteAsync(dynamic message)
        {
            await _router.RouteAsync(message);
        }
    }
}