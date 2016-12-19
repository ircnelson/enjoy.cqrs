// The MIT License (MIT)
// 
// Copyright (c) 2016 Nelson Corrêa V. Júnior
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Events;
using System.Reflection;

namespace EnjoyCQRS.MessageBus.InProcess
{
    public class EventPublisher : IEventPublisher
    {
        private readonly IEventRouter _router;

        public EventPublisher(IEventRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));

            _router = router;
        }

        private readonly Queue<object> _queue = new Queue<object>();

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

        private Task Enqueue(object message)
        {
            _queue.Enqueue(message);

            return Task.CompletedTask;
        }

        public async Task CommitAsync()
        {
            while (_queue.Count > 0)
            {
                var message = _queue.Dequeue();
                
                await _router.RouteAsync((dynamic)message);
            }
        }

        public void Rollback()
        {
            _queue.Clear();
        }
    }
}