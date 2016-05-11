using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

        public void Route(ICommand command)
        {
            var genericCommandHandler = typeof (ICommandHandler<>).MakeGenericType(command.GetType());
            var enumerabeGenericCommandHandler = typeof (IEnumerable<>).MakeGenericType(genericCommandHandler);

            var handlers = _scope.ResolveOptional(enumerabeGenericCommandHandler) as IEnumerable;


            foreach (var handler in handlers)
            {
                
                var methodInfo = handler.GetType().GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public);
                methodInfo.Invoke(handler, new[] {command});
            }
            
        }
    }
}