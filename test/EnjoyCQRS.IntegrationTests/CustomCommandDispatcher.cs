using System;
using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.MessageBus;

namespace EnjoyCQRS.IntegrationTests
{
    public class CustomCommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomCommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());

            var handler = (dynamic) _serviceProvider.GetService(handlerType);

            await handler.ExecuteAsync(command);
        }
    }
}