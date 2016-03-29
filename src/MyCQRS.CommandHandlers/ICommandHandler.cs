using MyCQRS.Commands;

namespace MyCQRS.CommandHandlers
{
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        void Execute(TCommand command);
    }
}
