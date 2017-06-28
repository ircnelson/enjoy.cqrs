using EnjoyCQRS.Commands;
using EnjoyCQRS.MessageBus;
using System.Threading.Tasks;

namespace EnjoyCQRS.UnitTests.Core.Stubs
{
    class DefaultCommandDispatcher : ICommandDispatcher
    {
        public Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            return Task.CompletedTask;
        }
    }
}
