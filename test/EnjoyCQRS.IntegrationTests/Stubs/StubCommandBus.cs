using System.Threading.Tasks;
using Autofac;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class StubCommandBus : ICommandDispatcher
    {
        private readonly ILifetimeScope _scope;

        public StubCommandBus(ILifetimeScope scope)
        {
            _scope = scope;
        }
        
        public async Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            /*
             * If you try to resolve service based on ICommandHandler<TCommand>, 
             * the result will be ICommandHandler<ICommand> then nothing has be found. :(
             * 
             * Use dynamic cast or use MakeGeneric
             */
            await Routing((dynamic) command);
        }

        private async Task Routing<TCommand>(TCommand command) where TCommand : ICommand
        {
            var handler = _scope.Resolve<ICommandHandler<TCommand>>();

            await handler.ExecuteAsync(command);
        }
    }
}