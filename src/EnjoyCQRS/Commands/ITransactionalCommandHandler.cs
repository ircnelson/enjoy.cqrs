using System;

namespace EnjoyCQRS.Commands
{
    public interface ITransactionalCommandHandler
    {
        Action<TCommand> Factory<TCommand, TCommandHandler>(TCommandHandler commandHandler)
            where TCommand : class, ICommand
            where TCommandHandler : ICommandHandler<TCommand>;
    }
}