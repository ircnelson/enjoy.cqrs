using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Bus.InProcess
{
    public class CommandBus : InProcessBus, ICommandDispatcher
    {
        private readonly ICommandRouter _router;

        public CommandBus(ICommandRouter router)
        {
            _router = router;
        }
        
        public async Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            await SendAsync(command);
        }

        protected override async Task RouteAsync(dynamic message)
        {
            await _router.RouteAsync(message);
        }
    }
}