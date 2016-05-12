using System.Collections.Generic;
using Autofac;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class CommandRouter : ICommandRouter
    {
        private readonly ILifetimeScope _scope;

        public CommandRouter(ILifetimeScope scope)
        {
            _scope = scope;
        }
        
        public void Route<TCommand>(TCommand command) where TCommand : ICommand
        {
            var handler = _scope.Resolve<ICommandHandler<TCommand>>();
            handler.Execute(command);
        }
    }
}