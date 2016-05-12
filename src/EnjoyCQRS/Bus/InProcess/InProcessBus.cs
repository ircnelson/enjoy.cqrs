using System.Collections.Generic;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Bus.InProcess
{
    public abstract class InProcessBus
    {
        private readonly object _lockObject = new object();
        private readonly Queue<IMessage> _preCommitQueue;
        private readonly InMemoryQueue<IMessage> _postCommitQueue;

        protected InProcessBus()
        {   
            _preCommitQueue = new Queue<IMessage>(32);
            _postCommitQueue = new InMemoryQueue<IMessage>();
            _postCommitQueue.Pop(DoPublish);
        }
        
        public void Send(IMessage command)
        {
            lock (_lockObject)
            {
                _preCommitQueue.Enqueue(command);
            }
        }

        public void Send(IEnumerable<IMessage> messages)
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

        protected abstract void Route(dynamic message);

        private void DoPublish(dynamic message)
        {
            try
            {
                Route(message);
            }
            finally
            {
                _postCommitQueue.Pop(DoPublish);
            }
        }
    }
}