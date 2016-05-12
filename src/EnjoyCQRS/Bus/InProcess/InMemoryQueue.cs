using System;
using System.Collections.Generic;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Bus.InProcess
{
    public interface IQueue<TMessage>
    {
        void Put(TMessage item);
        void Pop(Action<TMessage> popAction);
    }

    public class InMemoryQueue<TMessage> : IQueue<TMessage>
        where TMessage : IMessage
    {
        private readonly Queue<TMessage> _itemQueue;
        private readonly Queue<Action<TMessage>> _listenerQueue;

        public InMemoryQueue()
        {
            _itemQueue = new Queue<TMessage>(32);
            _listenerQueue = new Queue<Action<TMessage>>(32);
        }

        public void Put(TMessage item)
        {
            if (_listenerQueue.Count == 0)
            {
                _itemQueue.Enqueue(item);
                return;
            }

            var listener = _listenerQueue.Dequeue();
            listener(item);
        }

        public void Pop(Action<TMessage> popAction)
        {
            if (_itemQueue.Count == 0)
            {
                _listenerQueue.Enqueue(popAction);
                return;
            }

            var item = _itemQueue.Dequeue();
            popAction(item);
        }
    }
}