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

using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EnjoyCQRS.Core
{
    public class EnjoyDependencies
    {
        public Dependency EventStore { get; } = Dependency.Of<IEventStore>();
        public Dependency CommandDispatcher { get; } = Dependency.Of<ICommandDispatcher>();
        public Dependency UnitOfWork { get; } = Dependency.Of<IUnitOfWork>();
        public Dependency EventRouter { get; } = Dependency.Of<IEventRouter>();
        public Dependency SnapshotStrategy { get; } = Dependency.Of<ISnapshotStrategy>();
        public Dependency EventPublisher { get; } = Dependency.Of<IEventPublisher>();
        public Dependency Session { get; } = Dependency.Of<ISession>();
        public Dependency Repository { get; } = Dependency.Of<IRepository>();
        public Dependency EventUpdateManager { get; } = Dependency.Of<IEventUpdateManager>();
        public Dependency LoggerFactory { get; } = Dependency.Of<ILoggerFactory>();
        public Dependency EventsMetadataService { get; } = Dependency.Of<IEventsMetadataService>();

        public IEnumerable<Dependency> All => GetType()
            .GetTypeInfo()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .Where(p => p.PropertyType == typeof(Dependency))
            .Select(p => p.GetValue(this))
            .Cast<Dependency>();

        public IEnumerable<Dependency> NotRegistered => All.Where(d => d.State == DependencyState.NotRegistered).ToList();
        public IEnumerable<Dependency> Registered => All.Where(d => d.State == DependencyState.Registered).ToList();

        public bool IsValid()
        {
            return !NotRegistered.Any();
        }
    }
}
