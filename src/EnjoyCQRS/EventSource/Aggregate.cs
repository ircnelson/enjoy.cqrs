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
using EnjoyCQRS.Collections;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.EventSource
{
	public abstract class Aggregate : IAggregate
	{
		private readonly List<UncommittedEvent> _uncommittedEvents = new List<UncommittedEvent>();
		private readonly Route<IDomainEvent> _routeEvents;

		protected virtual bool HandleIsMandatory { get; } = true;

		/// <summary>
		/// Collection of <see cref="IDomainEvent"/> that contains uncommitted events.
		/// All events that not persisted yet should be here.
		/// </summary>
		public IReadOnlyCollection<IUncommittedEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

		/// <summary>
		/// Unique identifier.
		/// </summary>
		public Guid Id { get; protected set; }

		/// <summary>
		/// Current version of the Aggregate.
		/// </summary>
		public int Version { get; protected set; }

		/// <summary>
		/// This version is calculated based on Version + Uncommitted events count.
		/// </summary>
		public int Sequence => Version + _uncommittedEvents.Count;

		/// <summary>
		/// Aggregate default constructor.
		/// </summary>
		protected Aggregate()
		{
			_routeEvents = new Route<IDomainEvent>(HandleIsMandatory);

			RegisterEvents();

		}

		/// <summary>
		/// This method is called internaly and you can put all handlers here.
		/// </summary>
		protected abstract void RegisterEvents();

		protected void SubscribeTo<T>(Action<T> action)
			where T : class, IDomainEvent
		{
			_routeEvents.Add(typeof(T), o => action(o as T));
		}

		/// <summary>
		/// Event emitter.
		/// </summary>
		/// <param name="event"></param>
		protected void Emit(IDomainEvent @event)
		{
			ApplyEvent(@event, true);
		}

		/// <summary>
		/// Apply the event in Aggregate and store the event in Uncommitted list.
		/// The last event applied is the current state of the Aggregate.
		/// </summary>
		/// <param name="event"></param>
		private void ApplyEvent(IDomainEvent @event, bool isNew = false)
		{
			ApplyEvent(@event);

			if (isNew)
			{
				Task.WaitAll(Task.Delay(1));

				_uncommittedEvents.Add(new UncommittedEvent(this, @event, Sequence + 1));
			}
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
		/// Clear the collection of events that uncommitted.
		/// </summary>
		public void ClearUncommittedEvents()
		{
			_uncommittedEvents.Clear();
		}

		/// <summary>
		/// Load the events in the Aggregate.
		/// </summary>
		/// <param name="domainEvents"></param>
		public void LoadFromHistory(CommittedEventsCollection domainEvents)
		{
			foreach (var @event in domainEvents)
			{
				ApplyEvent(@event);
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