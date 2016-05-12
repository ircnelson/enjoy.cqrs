using EnjoyCQRS.Commands;
using EnjoyCQRS.Messages;

namespace EnjoyCQRS.Bus.InProcess
{
    public class CommandBus : InProcessBus<ICommand>, ICommandDispatcher
    {
        private readonly ICommandRouter _router;

        public CommandBus(ICommandRouter router)
        {
            _router = router;
        }

        protected override void Route(ICommand message)
        {
            _router.Route(message);
        }

        public void Dispatch<TCommand>(TCommand command) where TCommand : ICommand
        {
            Send(command);
        }
    }
}