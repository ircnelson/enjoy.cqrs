using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.Commands
{
    public class TransactionalCommandHandler : ITransactionalCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionalCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Action<TCommand> Factory<TCommand, TCommandHandler>(TCommandHandler commandHandler) 
            where TCommand : class, ICommand 
            where TCommandHandler : ICommandHandler<TCommand>
        {
            return cmd => new TransactionHandler<TCommand, TCommandHandler>(_unitOfWork).Execute(cmd, commandHandler);
        }
    }
}