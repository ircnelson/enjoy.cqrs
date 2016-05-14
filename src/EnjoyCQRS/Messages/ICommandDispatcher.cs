using System.Threading.Tasks;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.Messages
{
    public interface ICommandDispatcher
    {
        /// <summary>
        /// Dispatch the command to the handler.
        /// </summary>
        Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand;
    }
}