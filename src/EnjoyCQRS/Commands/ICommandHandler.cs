using System.Threading.Tasks;

namespace EnjoyCQRS.Commands
{
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        Task ExecuteAsync(TCommand command);
    }
}
