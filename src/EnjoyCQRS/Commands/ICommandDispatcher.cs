using EnjoyCQRS.Bus;

namespace EnjoyCQRS.Commands
{
    public interface ICommandDispatcher : IUnitOfWork
    {
        void Dispatch<TCommand>(TCommand message) where TCommand : ICommand;
    }
}