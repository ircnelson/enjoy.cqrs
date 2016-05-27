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
using System.Linq;
using EnjoyCQRS.Collections;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventSource
{
    public abstract class Aggregate : IAggregate
    {
        private readonly List<IDomainEvent> _uncommitedEvents = new List<IDomainEvent>();
        private readonly Route<IDomainEvent> _routeEvents = new Route<IDomainEvent>();

        /// <summary>
        /// Collection of <see cref="IDomainEvent"/> that contains uncommited events.
        /// All events that not persisted yet should be here.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> UncommitedEvents => _uncommitedEvents.AsReadOnly();
        
        /// <summary>
        /// Unique identifier.
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Current version of the Aggregate.
        /// </summary>
        public int Version { get; protected set; }

        /// <summary>
        /// This version is calculated based on Version + Uncommited events count.
        /// </summary>
        public int EventVersion => Version + _uncommitedEvents.Count;

        /// <summary>
        /// Aggregate default constructor.
        /// </summary>
        protected Aggregate()
        {
            RegisterEvents();
        }

        /// <summary>
        /// This method is called internaly and you can put all handlers here.
        /// </summary>
        protected abstract void RegisterEvents();

        protected void SubscribeTo<T>(Action<T> action)
            where T : class 
        {
            _routeEvents.Add(typeof(T), o => action(o as T));
        }

        /// <summary>
        /// Event emitter.
        /// </summary>
        /// <param name="event"></param>
        protected void Emit(IDomainEvent @event)
        {
            ApplyEvent(new UncommitedDomainEvent(@event, EventVersion + 1));
        }

        /// <summary>
        /// Apply the event in Aggregate and store the event in Uncommited list.
        /// The last event applied is the current state of the Aggregate.
        /// </summary>
        /// <param name="event"></param>
        private void ApplyEvent(UncommitedDomainEvent @event)
        {
            ApplyEvent(@event.OriginalEvent);
            
            _uncommitedEvents.Add(@event.OriginalEvent);
        }

        /// <summary>
        /// Apply the event in Aggregate.
        /// The last event applied is the current state of the Aggregate.
        /// </summary>
        /// <param name="event"></param>
        private void ApplyEvent(IDomainEvent @event)
        {
            _routeEvents.Handle(@event);
        }

        /// <summary>
        /// Clear the collection of events that uncommited.
        /// </summary>
        public void ClearUncommitedEvents()
        {
            _uncommitedEvents.Clear();
        }

        /// <summary>
        /// Load the events in the Aggregate.
        /// </summary>
        /// <param name="domainEvents"></param>
        public void LoadFromHistory(CommitedDomainEventCollection domainEvents)
        {
            foreach (var @event in domainEvents)
            {
                ApplyEvent(@event);
            }
            
            if (domainEvents.Any())
            {
                UpdateVersion(domainEvents.Max(e => e.Version));
            }
        }

        /// <summary>
        /// Update aggregate's version.
        /// </summary>
        /// <param name="version"></param>
        internal void UpdateVersion(int version)
        {
            Version = version;
        }
    }
}