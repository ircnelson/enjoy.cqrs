using System.Threading.Tasks;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.IntegrationTests.Infrastructure
{
    public class StubCommandDispatcher : CommandDispatcher
    {
        private readonly ILifetimeScope _scope;

        public StubCommandDispatcher(ILifetimeScope scope)
        {
            _scope = scope;
        }
        
        protected override async Task RouteAsync<TCommand>(TCommand command)
        {
            var handler = _scope.Resolve<ICommandHandler<TCommand>>();

            await handler.ExecuteAsync(command);
        }
    }
}