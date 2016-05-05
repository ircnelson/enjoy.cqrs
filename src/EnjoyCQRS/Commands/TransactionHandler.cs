using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.Commands
{
    /// <summary>
    /// Keep the <see cref="TCommandHandler"/> in transaction.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TCommandHandler"></typeparam>
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