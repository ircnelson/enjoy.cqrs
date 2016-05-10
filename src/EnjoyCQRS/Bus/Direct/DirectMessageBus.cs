using System.Collections.Generic;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Bus.Direct
{
    public class DirectMessageBus : ICommandDispatcher, IEventPublisher
    {
        private readonly ICommandRouter _commandRouter;
        private readonly IEventRouter _eventRouter;
        
        private readonly object _lockObject = new object();
        private readonly Queue<object> _preCommitQueue;
        private readonly InMemoryQueue _postCommitQueue;

        public DirectMessageBus(ICommandRouter commandRouter, IEventRouter eventRouter)
        {
            _commandRouter = commandRouter;
            _eventRouter = eventRouter;
            
            _preCommitQueue = new Queue<object>(32);
            _postCommitQueue = new InMemoryQueue();
            _postCommitQueue.Pop(DoPublish);
        }

        public void Dispatch<TCommand>(TCommand command) where TCommand : ICommand
        {
            lock (_lockObject)
            {
                _preCommitQueue.Enqueue(command);
            }
        }

        public void Dispatch<TCommand>(IEnumerable<TCommand> commands) where TCommand : ICommand
        {
            foreach (var command in commands)
            {
                Dispatch(command);
            }
        }

        public void Publish<TEvent>(TEvent message) where TEvent : IDomainEvent
        {
            lock (_lockObject)
            {
                _preCommitQueue.Enqueue(message);
            }
        }

        public void Publish<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent
        {
            foreach (var message in messages)
            {
                Publish(message);
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
                if (message is ICommand)
                    _commandRouter.Route((ICommand) message);

                if (message is IDomainEvent)
                    _eventRouter.Route((IDomainEvent)message);
            }
            finally
            {
                _postCommitQueue.Pop(DoPublish);
            }
        }
    }
}