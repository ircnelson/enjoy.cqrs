using System;
using EnjoyCQRS.EventStore;

namespace EnjoyCQRS.Commands
{
    public class TransactionHandler<TCommand, TCommandHandler>
        where TCommandHandler : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Execute(TCommand command, TCommandHandler commandHandler)
        {
            try
            {
                commandHandler.Execute(command);
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
                throw;
            }
        }
    }
}