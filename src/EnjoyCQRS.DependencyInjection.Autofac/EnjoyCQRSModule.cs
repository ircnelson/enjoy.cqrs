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
using EnjoyCQRS.DependencyInjection.Autofac;
using EnjoyCQRS.DependencyInjection.Autofac.Core;
using EnjoyCQRS.DependencyInjection.Autofac.MessageBus;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus.InProcess;
using System;

namespace EnjoyCQRS.DependencyInjection.AutofacExtensions
{
    public class EnjoyCQRSModule : Module
    {
        private Func<IEnjoyDependenciesBuilder<IComponentContext>, IEnjoyDependenciesBuilder<IComponentContext>> _options;

        public EnjoyCQRSModule()
        {
        }

        public EnjoyCQRSModule(Func<IEnjoyDependenciesBuilder<IComponentContext>, IEnjoyDependenciesBuilder<IComponentContext>> options)
        {
            _options = options;
        }

        protected override void Load(ContainerBuilder builder)
        {
            IEnjoyDependenciesBuilder<IComponentContext> enjoyDependenciesBuilder = new AutofacEnjoyDependenciesBuilder(builder);
            
            if (_options != null)
            {
                enjoyDependenciesBuilder = _options(enjoyDependenciesBuilder);
            }

            enjoyDependenciesBuilder.WithEventStore<InMemoryEventStore>();
            enjoyDependenciesBuilder.WithCommandDispatcher<DefaultCommandDispatcher>();
            enjoyDependenciesBuilder.WithEventRouter<DefaultEventRouter>();
            enjoyDependenciesBuilder.WithEventPublisher<EventPublisher>();
            enjoyDependenciesBuilder.WithSession<Session>();
            enjoyDependenciesBuilder.WithUnitOfWork<UnitOfWork>();
            enjoyDependenciesBuilder.WithEventsMetadataService<EventsMetadataService>();
            enjoyDependenciesBuilder.WithRepository<Repository>();
            enjoyDependenciesBuilder.WithSnapshotStrategy<IntervalSnapshotStrategy>();
            enjoyDependenciesBuilder.WithEventUpdateManager<EventUpdateManager>();
            enjoyDependenciesBuilder.WithLoggerFactory<NoopLoggerFactory>();

            var enjoyDependencies = enjoyDependenciesBuilder.Build();
        }
    }
}
