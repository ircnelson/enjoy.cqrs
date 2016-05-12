using System.Collections.Generic;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Bus.InProcess
{
    public abstract class InProcessBus<TMessage>
        where TMessage : IMessage
    {
        private readonly object _lockObject = new object();
        private readonly Queue<TMessage> _preCommitQueue;
        private readonly InMemoryQueue<TMessage> _postCommitQueue;

        protected InProcessBus()
        {   
            _preCommitQueue = new Queue<TMessage>(32);
            _postCommitQueue = new InMemoryQueue<TMessage>();
            _postCommitQueue.Pop(DoPublish);
        }
        
        public void Send(TMessage command)
        {
            lock (_lockObject)
            {
                _preCommitQueue.Enqueue(command);
            }
        }

        public void Send(IEnumerable<TMessage> messages)
        {
            foreach (var message in messages)
            {
                Send(message);
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

        protected abstract void Route(TMessage message);

        private void DoPublish(TMessage message)
        {
            try
            {
                //if (message is ICommand)
                //    _commandRouter.Route((ICommand) message);

                //if (message is IDomainEvent)
                //    _eventRouter.Route((IDomainEvent)message);
                Route(message);
            }
            finally
            {
                _postCommitQueue.Pop(DoPublish);
            }
        }
    }
}