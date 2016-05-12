using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.Commands
{
    /// <summary>
    /// Keep the <see cref="ICommandHandler{TCommand}" /> in transaction.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public class TransactionalCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _commandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionalCommandHandler(
            ICommandHandler<TCommand> commandHandler,
            IUnitOfWork unitOfWork)
        {
            _commandHandler = commandHandler;
            _unitOfWork = unitOfWork;
        }

        public void Execute(TCommand command)
        {
            _commandHandler.Execute(command);

            _unitOfWork.Commit();
        }
    }
}