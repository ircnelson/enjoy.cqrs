using System.Threading.Tasks;
using EnjoyCQRS.Commands;

namespace EnjoyCQRS.Bus
{
    public interface ICommandRouter
    {
        Task RouteAsync<TCommand>(TCommand command) where TCommand : ICommand;
    }
}