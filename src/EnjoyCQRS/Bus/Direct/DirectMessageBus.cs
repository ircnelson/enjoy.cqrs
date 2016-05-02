using System.Collections.Generic;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Bus.Direct
{
    public class DirectMessageBus : ICommandDispatcher, IEventPublisher
    {
        private readonly IRouterMessages _routeMessages;
        private readonly object _lockObject = new object();
        private readonly Queue<object> _preCommitQueue;
        private readonly InMemoryQueue _postCommitQueue;

        public DirectMessageBus(IRouterMessages routeMessages)
        {
            _routeMessages = routeMessages;
            _preCommitQueue = new Queue<object>(32);
            _postCommitQueue = new InMemoryQueue();
            _postCommitQueue.Pop(DoPublish);
        }

        public void Dispatch<TCommand>(TCommand message) where TCommand : ICommand
        {
            lock (_lockObject)
            {
                _preCommitQueue.Enqueue(message);
            }
        }

        public void Publish<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent
        {
            lock (_lockObject)
            {
                foreach (var message in messages)
                {
                    _preCommitQueue.Enqueue(message);
                }
            }
        }

        public void Commit()
        {
            lock (_lockObject)
            {
                while (_preCommitQueue.Count > 0)
                {
                    _postCommitQueue.Put(_preCommitQueue.Dequeue());
                }
            }
        }

        public void Rollback()
        {
            lock (_lockObject)
            {
                _preCommitQueue.Clear();
            }
        }

        private void DoPublish(object message)
        {
            try
            {
                _routeMessages.Route(message);
            }
            finally
            {
                _postCommitQueue.Pop(DoPublish);
            }
        }
    }
}