using EnjoyCQRS.Core;
using Xunit;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.EventSource;
using EnjoyCQRS.EventSource.Snapshots;
using EnjoyCQRS.EventSource.Projections;
using EnjoyCQRS.Logger;
using EnjoyCQRS.MessageBus.InProcess;
using System.Linq;
using FluentAssertions;
using EnjoyCQRS.UnitTests.Core.Stubs;
using System;
using EnjoyCQRS.MessageBus;
using Moq;

namespace EnjoyCQRS.UnitTests.Core
{
    [Trait("Unit", "Dependencies")]
    public class EnjoyDependenciesBuilderTests
    {
        private const int FrameworkDependencies = 11;
        
        [Fact]
        public void Throw_exception_When_some_dependency_not_registered()
        {
            // Arrange

            IEnjoyDependenciesBuilder<IServiceProvider> enjoyDependenciesBuilder = new StubEnjoyDependenciesBuilder();

            // Act

            Action act = () => enjoyDependenciesBuilder.Build();

            // Assert

            act.ShouldThrowExactly<AggregateException>().And.InnerExceptions.Count().Should().Be(FrameworkDependencies);
        }
        
        [Fact]
        public void Should_register_default_dependencies_with_generics()
        {
            // Arrange

            IEnjoyDependenciesBuilder<IServiceProvider> enjoyDependenciesBuilder = new StubEnjoyDependenciesBuilder();

            // Act

            enjoyDependenciesBuilder.WithEventStore<InMemoryEventStore>();
            enjoyDependenciesBuilder.WithCommandDispatcher<DefaultCommandDispatcher>();
            enjoyDependenciesBuilder.WithEventRouter<DefaultEventRouter>();
            enjoyDependenciesBuilder.WithEventUpdateManager<DefaultEventUpdateManager>();
            enjoyDependenciesBuilder.WithEventPublisher<EventPublisher>();
            enjoyDependenciesBuilder.WithUnitOfWork<UnitOfWork>();
            enjoyDependenciesBuilder.WithSession<Session>();
            enjoyDependenciesBuilder.WithRepository<Repository>();
            enjoyDependenciesBuilder.WithSnapshotStrategy<IntervalSnapshotStrategy>();
            enjoyDependenciesBuilder.WithLoggerFactory<NoopLoggerFactory>();
            enjoyDependenciesBuilder.WithEventsMetadataService<EventsMetadataService>();

            // Assert

            var dependencies = enjoyDependenciesBuilder.Build();

            dependencies.IsValid().Should().BeTrue();
        }

        [Fact]
        public void Should_register_default_dependencies_with_type()
        {
            // Arrange

            IEnjoyDependenciesBuilder<IServiceProvider> enjoyDependenciesBuilder = new StubEnjoyDependenciesBuilder();

            // Act

            enjoyDependenciesBuilder.WithEventStore(typeof(InMemoryEventStore));
            enjoyDependenciesBuilder.WithCommandDispatcher(typeof(DefaultCommandDispatcher));
            enjoyDependenciesBuilder.WithEventRouter(typeof(DefaultEventRouter));
            enjoyDependenciesBuilder.WithEventUpdateManager(typeof(DefaultEventUpdateManager));
            enjoyDependenciesBuilder.WithEventPublisher(typeof(EventPublisher));
            enjoyDependenciesBuilder.WithUnitOfWork(typeof(UnitOfWork));
            enjoyDependenciesBuilder.WithSession(typeof(Session));
            enjoyDependenciesBuilder.WithRepository(typeof(Repository));
            enjoyDependenciesBuilder.WithSnapshotStrategy(typeof(IntervalSnapshotStrategy));
            enjoyDependenciesBuilder.WithLoggerFactory(typeof(NoopLoggerFactory));
            enjoyDependenciesBuilder.WithEventsMetadataService(typeof(EventsMetadataService));

            // Assert

            var dependencies = enjoyDependenciesBuilder.Build();

            dependencies.IsValid().Should().BeTrue();
        }
        
        [Fact]
        public void Should_register_default_dependencies_with_instance_factory()
        {
            // Arrange

            IEnjoyDependenciesBuilder<IServiceProvider> enjoyDependenciesBuilder = new StubEnjoyDependenciesBuilder();

            // Act

            enjoyDependenciesBuilder.WithEventStore((c) => Mock.Of<IEventStore>());
            enjoyDependenciesBuilder.WithCommandDispatcher((c) => Mock.Of<ICommandDispatcher>());
            enjoyDependenciesBuilder.WithEventRouter((c) => Mock.Of<IEventRouter>());
            enjoyDependenciesBuilder.WithEventUpdateManager((c) => Mock.Of<IEventUpdateManager>());
            enjoyDependenciesBuilder.WithEventPublisher((c) => Mock.Of<IEventPublisher>());
            enjoyDependenciesBuilder.WithUnitOfWork((c) => Mock.Of<IUnitOfWork>());
            enjoyDependenciesBuilder.WithSession((c) => Mock.Of<ISession>());
            enjoyDependenciesBuilder.WithRepository((c) => Mock.Of<IRepository>());
            enjoyDependenciesBuilder.WithSnapshotStrategy((c) => Mock.Of<ISnapshotStrategy>());
            enjoyDependenciesBuilder.WithLoggerFactory((c) => Mock.Of<ILoggerFactory>());
            enjoyDependenciesBuilder.WithEventsMetadataService((c) => Mock.Of<IEventsMetadataService>());

            // Assert

            var dependencies = enjoyDependenciesBuilder.Build();

            dependencies.IsValid().Should().BeTrue();
        }
    }
}
