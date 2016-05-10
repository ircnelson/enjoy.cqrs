using EnjoyCQRS.Commands;

namespace EnjoyCQRS.Bus
{
    public interface ICommandRouter
    {
        void Route(ICommand command);
    }
}