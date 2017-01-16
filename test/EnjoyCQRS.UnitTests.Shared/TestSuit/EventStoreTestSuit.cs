using System;
using System.Threading.Tasks;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus.InProcess;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.FooAggregate;
using FluentAssertions;

namespace EnjoyCQRS.UnitTests.Shared.TestSuit
{
    public class EventStoreTestSuit
    {
        private readonly IProjectionSerializer _projectionSerializer = new ProjectionSerializer();
        private readonly EventStoreWrapper _eventStore;
        
        public EventStoreTestSuit(IEventStore eventStore, IProjectionSerializer projectionSerializer = null)
        {
            _projectionSerializer = projectionSerializer;

            _eventStore = new EventStoreWrapper(eventStore);
        }

        public async Task EventTestsAsync()
        {
            var bar = GenerateBar();

            var session = CreateSession();

            await session.AddAsync(bar).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            session = CreateSession();

            var bar2 = await session.GetByIdAsync<Bar>(bar.Id).ConfigureAwait(false);

            var result = _eventStore.CalledMethods.HasFlag(EventStoreMethods.Ctor
                | EventStoreMethods.BeginTransaction
                | EventStoreMethods.SaveAsync
                | EventStoreMethods.SaveAggregateProjection
                | EventStoreMethods.CommitAsync
                | EventStoreMethods.GetAllEventsAsync);

            bar.Id.Should().Be(bar2.Id);

            result.Should().BeTrue();
        }

        public async Task SnapshotTestsAsync()
        {
            var foo = GenerateFoo(9);

            var session = CreateSession();

            await session.AddAsync(foo).ConfigureAwait(false);
            await session.SaveChangesAsync().ConfigureAwait(false);

            session = CreateSession();

            var foo2 = await session.GetByIdAsync<Foo>(foo.Id).ConfigureAwait(false);
            
            var result = _eventStore.CalledMethods.HasFlag(
                EventStoreMethods.Ctor 
                | EventStoreMethods.SaveAsync
                | EventStoreMethods.SaveAggregateProjection
                | EventStoreMethods.SaveSnapshotAsync 
                | EventStoreMethods.CommitAsync 
                | EventStoreMethods.GetLatestSnapshotByIdAsync 
                | EventStoreMethods.GetEventsForwardAsync);

            foo.Id.Should().Be(foo2.Id);

            result.Should().BeTrue();
        }

        public async Task DoSomeProblemAsync()
        {
            var foo = GenerateFoo();

            var session = CreateFaultSession();

            await session.AddAsync(foo).ConfigureAwait(false);
            try
            {
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception)
            {
                var result = _eventStore.CalledMethods.HasFlag(EventStoreMethods.Ctor 
                    | EventStoreMethods.BeginTransaction 
                    | EventStoreMethods.SaveAsync 
                    | EventStoreMethods.Rollback);

                result.Should().BeTrue();
            }
        }

        private ISession CreateSession()
        {
            var session = new Session(new NoopLoggerFactory(), _eventStore, new EventPublisher(StubEventRouter.Ok()), new EventSerializer(new JsonTextSerializer()), new SnapshotSerializer(new JsonTextSerializer()), _projectionSerializer, null, null, new IntervalSnapshotStrategy(10));

            return session;
        }

        private ISession CreateFaultSession()
        {
            var faultSession = new Session(new NoopLoggerFactory(), _eventStore, new EventPublisher(StubEventRouter.Fault()), new EventSerializer(new JsonTextSerializer()), new SnapshotSerializer(new JsonTextSerializer()), _projectionSerializer, null, null, new IntervalSnapshotStrategy(10));

            return faultSession;
        }

        private static Foo GenerateFoo(int quantity = 10)
        {
            var foo = new Foo(Guid.NewGuid());

            for (var i = 0; i < quantity; i++)
            {
                foo.DoSomething();
            }

            return foo;
        }

        private static Bar GenerateBar(int quantity = 10)
        {
            var bar = Bar.Create(Guid.NewGuid());

            for (var i = 0; i < quantity; i++)
            {
                bar.Speak($"Hello number {i}.");
            }

            return bar;
        }
    }
}