using System.Collections.Generic;
using System.Threading.Tasks;
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

            Initialize();
        }

        private async void Initialize()
        {
            await _postCommitQueue.PopAsync(DoPublish);
        }
        
        protected async Task SendAsync(IMessage command)
        {
            _preCommitQueue.Enqueue(command);
        }

        protected async Task SendAsync(IEnumerable<IMessage> messages)
        {
            foreach (var message in messages)
            {
                await SendAsync(message);
            }
        }
        
        public async Task CommitAsync()
        {
            while (_preCommitQueue.Count > 0)
            {
                await _postCommitQueue.PutAsync(_preCommitQueue.Dequeue());
            }
        }

        public void Rollback()
        {
            lock (_lockObject)
            {
                _preCommitQueue.Clear();
            }
        }

        protected abstract Task RouteAsync(dynamic message);

        private async void DoPublish(dynamic message)
        {
            try
            {
                await RouteAsync(message);
            }
            finally
            {
                await _postCommitQueue.PopAsync(DoPublish);
            }
        }
    }
}