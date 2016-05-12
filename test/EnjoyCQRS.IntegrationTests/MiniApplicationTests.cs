using System;
using System.Collections.Generic;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Fixtures;
using EnjoyCQRS.IntegrationTests.Stubs;
using FluentAssertions;
using Xunit;

namespace EnjoyCQRS.IntegrationTests
{
    public class MiniApplicationTests : IClassFixture<MiniApplicationFixture>
    {
        private const string CategoryName = "Integration";
        private const string CategoryValue = "Simulation";

        private readonly MiniApplicationFixture _fixture;

        public MiniApplicationTests(MiniApplicationFixture fixture)
        {
            _fixture = fixture;
        }
        
        [Fact]
        [Trait(CategoryName, CategoryValue)]
        public void Should_simulate_application()
        {
            using (var scope = _fixture.Container.BeginLifetimeScope())
            {
                var command = new CreateFakePersonCommand(Guid.NewGuid(), "Fake");

                DoDispatch(scope, command);
                
                var repository = scope.Resolve<IRepository>();
                
                var aggregateFromRepository = repository.GetById<FakePerson>(command.AggregateId);

                aggregateFromRepository.Should().NotBeNull();
            }
        }
        
        private void DoDispatch(ILifetimeScope scope, ICommand command)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandDispatcher = scope.Resolve<ICommandDispatcher>();

            commandDispatcher.Dispatch(command);
            commandDispatcher.Commit();
        }
    }
}