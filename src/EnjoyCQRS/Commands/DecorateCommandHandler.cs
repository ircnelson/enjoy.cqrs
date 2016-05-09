using System;
using EnjoyCQRS.EventSource;

namespace EnjoyCQRS.Commands
{
    public class DecorateCommandHandler : IDecorateCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public DecorateCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Action<TCommand> Decorate<TCommand, TCommandHandler>(TCommandHandler commandHandler) 
            where TCommand : class, ICommand 
            where TCommandHandler : ICommandHandler<TCommand>
        {
            return cmd => new TransactionHandler<TCommand, TCommandHandler>(_unitOfWork).Execute(cmd, commandHandler);
        }
    }
}