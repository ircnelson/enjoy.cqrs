using System;

namespace EnjoyCQRS.Commands
{
    public interface IDecorateCommandHandler
    {
        Action<TCommand> Decorate<TCommand, TCommandHandler>(TCommandHandler commandHandler)
            where TCommand : class, ICommand
            where TCommandHandler : ICommandHandler<TCommand>;
    }
}