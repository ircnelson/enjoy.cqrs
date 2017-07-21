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
using EnjoyCQRS.MessageBus;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;

namespace EnjoyCQRS.Core
{
    public interface IEnjoyDependenciesBuilder<TResolver>
    {
        IEnjoyDependenciesBuilder<TResolver> WithEventStore<TImplementation>() where TImplementation : class, IEventStore;
        IEnjoyDependenciesBuilder<TResolver> WithEventStore<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventStore;
        IEnjoyDependenciesBuilder<TResolver> WithEventStore(Type type);

        IEnjoyDependenciesBuilder<TResolver> WithCommandDispatcher<TImplementation>() where TImplementation : class, ICommandDispatcher;
        IEnjoyDependenciesBuilder<TResolver> WithCommandDispatcher<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, ICommandDispatcher;
        IEnjoyDependenciesBuilder<TResolver> WithCommandDispatcher(Type type);

        IEnjoyDependenciesBuilder<TResolver> WithUnitOfWork<TImplementation>() where TImplementation : class, IUnitOfWork;
        IEnjoyDependenciesBuilder<TResolver> WithUnitOfWork<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IUnitOfWork;
        IEnjoyDependenciesBuilder<TResolver> WithUnitOfWork(Type type);

        IEnjoyDependenciesBuilder<TResolver> WithEventRouter<TImplementation>() where TImplementation : class, IEventRouter;
        IEnjoyDependenciesBuilder<TResolver> WithEventRouter<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventRouter;
        IEnjoyDependenciesBuilder<TResolver> WithEventRouter(Type type);

        IEnjoyDependenciesBuilder<TResolver> WithEventPublisher<TImplementation>() where TImplementation : class, IEventPublisher;
        IEnjoyDependenciesBuilder<TResolver> WithEventPublisher<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventPublisher;
        IEnjoyDependenciesBuilder<TResolver> WithEventPublisher(Type type);
        
        IEnjoyDependenciesBuilder<TResolver> WithSnapshotStrategy<TImplementation>() where TImplementation : class, ISnapshotStrategy;
        IEnjoyDependenciesBuilder<TResolver> WithSnapshotStrategy<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, ISnapshotStrategy;
        IEnjoyDependenciesBuilder<TResolver> WithSnapshotStrategy(Type type);
        
        IEnjoyDependenciesBuilder<TResolver> WithSession<TImplementation>() where TImplementation : class, ISession;
        IEnjoyDependenciesBuilder<TResolver> WithSession<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, ISession;
        IEnjoyDependenciesBuilder<TResolver> WithSession(Type type);

        IEnjoyDependenciesBuilder<TResolver> WithRepository<TImplementation>() where TImplementation : class, IRepository;
        IEnjoyDependenciesBuilder<TResolver> WithRepository<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IRepository;
        IEnjoyDependenciesBuilder<TResolver> WithRepository(Type type);

        IEnjoyDependenciesBuilder<TResolver> AddMetadataProvider<TImplementation>() where TImplementation : class, IMetadataProvider;
        IEnjoyDependenciesBuilder<TResolver> AddMetadataProvider<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IMetadataProvider;
        IEnjoyDependenciesBuilder<TResolver> AddMetadataProvider(Type type);

        IEnjoyDependenciesBuilder<TResolver> WithEventUpdateManager<TImplementation>() where TImplementation : class, IEventUpdateManager;
        IEnjoyDependenciesBuilder<TResolver> WithEventUpdateManager<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventUpdateManager;
        IEnjoyDependenciesBuilder<TResolver> WithEventUpdateManager(Type type);

        IEnjoyDependenciesBuilder<TResolver> WithEventsMetadataService<TImplementation>() where TImplementation : class, IEventsMetadataService;
        IEnjoyDependenciesBuilder<TResolver> WithEventsMetadataService<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, IEventsMetadataService;
        IEnjoyDependenciesBuilder<TResolver> WithEventsMetadataService(Type type);

        IEnjoyDependenciesBuilder<TResolver> WithLoggerFactory<TImplementation>() where TImplementation : class, ILoggerFactory;
        IEnjoyDependenciesBuilder<TResolver> WithLoggerFactory<TImplementation>(Func<TResolver, TImplementation> instanceFactory) where TImplementation : class, ILoggerFactory;
        IEnjoyDependenciesBuilder<TResolver> WithLoggerFactory(Type type);

        EnjoyDependencies Build();
    }
}
