using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Bus.InProcess
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IEventRouter _router;

        private readonly Queue<dynamic> _queue = new Queue<dynamic>();

        public EventPublisher(IEventRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));

            _router = router;
        }

        public async Task PublishAsync<TEvent>(TEvent message) where TEvent : IDomainEvent
        {
            await Enqueue(message);
        }

        public async Task PublishAsync<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent
        {
            foreach (var message in messages)
            {
                await Enqueue(message);
            }
        }

        private Task Enqueue(dynamic message)
        {
            _queue.Enqueue(message);

            return Task.CompletedTask;
        }
        
        public async Task CommitAsync()
        {
            while (_queue.Count > 0)
            {
                var message = _queue.Dequeue();

                await _router.RouteAsync(message);
            }
        }

        public void Rollback()
        {
            _queue.Clear();
        }
    }
}