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

using EnjoyCQRS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnjoyCQRS.Stores.InMemory
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly List<ICommittedEvent> _events;
        private readonly List<IUncommittedEvent> _uncommittedEvents = new List<IUncommittedEvent>();
        
        public IEnumerable<ICommittedEvent> Events => _events.AsReadOnly();
        public IEnumerable<IUncommittedEvent> Uncommitted => _uncommittedEvents.AsReadOnly();

        public InMemoryEventStore(List<ICommittedEvent> storage)
        {
            _events = storage;
        }

        public virtual Task<IEnumerable<ICommittedEvent>> GetAllEventsAsync(Guid id)
        {
            var events = Events
            .Where(e => e.AggregateId == id)
            .OrderBy(e => e.Version)
            .ToList();

            return Task.FromResult<IEnumerable<ICommittedEvent>>(events);
        }

        public virtual Task<IEnumerable<ICommittedEvent>> GetEventsForwardAsync(Guid id, int version)
        {
            var events = Events
                .Where(e => e.AggregateId == id && e.Version > version)
                .OrderBy(e => e.Version)
                .ToList();

            return Task.FromResult<IEnumerable<ICommittedEvent>>(events);
        }

        public virtual Task AppendAsync(IEnumerable<IUncommittedEvent> uncommittedEvents)
        {
            _uncommittedEvents.AddRange(uncommittedEvents);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        internal void ClearUncommitted()
        {
            _uncommittedEvents.Clear();
        }
    }
}
