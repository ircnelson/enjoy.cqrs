using System.Collections.Generic;
using EnjoyCQRS.Bus;

namespace EnjoyCQRS.Commands
{
    public interface ICommandDispatcher : IUnitOfWork
    {
        /// <summary>
        /// Dispatch the command to the handler.
        /// </summary>
        void Dispatch<TCommand>(TCommand command) where TCommand : ICommand;

        /// <summary>
        /// Dispatch the commands to the handler.
        /// </summary>
        void Dispatch<TCommand>(IEnumerable<TCommand> commands) where TCommand : ICommand;
    }
}