using EnjoyCQRS.Bus;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.Messages
{
    public interface ICommandDispatcher : ITransactionalMessageBus
    {
        /// <summary>
        /// Dispatch the command to the handler.
        /// </summary>
        void Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
    }
}