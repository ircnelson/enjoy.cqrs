using System.Threading.Tasks;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.Owin.IntegrationTests.Infrastructure
{
    public class AutofacCommandDispatcher : CommandDispatcher
    {
        private readonly ILifetimeScope _scope;

        public AutofacCommandDispatcher(ILifetimeScope scope)
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