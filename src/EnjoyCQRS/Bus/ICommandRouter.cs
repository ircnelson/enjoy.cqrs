using EnjoyCQRS.Commands;

namespace EnjoyCQRS.Bus
{
    public interface ICommandRouter
    {
        void Route<TCommand>(TCommand command) where TCommand : ICommand;
    }
}