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

using Autofac;
using EnjoyCQRS.Core;
using EnjoyCQRS.Events;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EnjoyCQRS.DependencyInjection.Autofac.Core
{
    public class EventUpdateManager : IEventUpdateManager
    {
        private readonly ILifetimeScope _scope;
        private readonly Dictionary<Type, object> _eventUpdate = new Dictionary<Type, object>();

        public EventUpdateManager(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public IEnumerable<IDomainEvent> Update(IEnumerable<IDomainEvent> events)
        {
            using (var scope = _scope.BeginLifetimeScope())
            {
                var eventUpdateTypeOf = typeof(IEventUpdate<>);

                var updatedEvents = new List<IDomainEvent>();

                foreach (var @event in events)
                {
                    var eventType = @event.GetType();

                    object eventUpdate;

                    if (!_eventUpdate.ContainsKey(eventType))
                    {
                        eventUpdate = scope.ResolveOptional(eventUpdateTypeOf.MakeGenericType(eventType));

                        if (eventUpdate == null)
                        {
                            updatedEvents.Add(@event);

                            continue;
                        }

                        _eventUpdate.Add(eventType, eventUpdate);
                    }

                    eventUpdate = _eventUpdate[eventType];

                    var methodInfo = eventUpdate.GetType().GetMethod(nameof(IEventUpdate<object>.Update), BindingFlags.Instance | BindingFlags.Public);
                    var resultMethod = (IEnumerable<IDomainEvent>)methodInfo.Invoke(eventUpdate, new object[] { @event });

                    updatedEvents.AddRange(resultMethod);
                }

                return updatedEvents;
            }
        }
    }
}
