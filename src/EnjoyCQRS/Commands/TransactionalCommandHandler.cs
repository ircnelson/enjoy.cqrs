using System;
using System.Threading.Tasks;
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
            if (commandHandler == null) throw new ArgumentNullException(nameof(commandHandler));
            if (unitOfWork == null) throw new ArgumentNullException(nameof(unitOfWork));

            _commandHandler = commandHandler;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(TCommand command)
        {
            await _commandHandler.ExecuteAsync(command);

            await _unitOfWork.CommitAsync();
        }
    }
}