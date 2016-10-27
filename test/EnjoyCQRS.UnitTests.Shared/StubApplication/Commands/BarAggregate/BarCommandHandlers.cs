using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.BarAggregate;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.BarAggregate
{
    public class BarCommandHandlers : ICommandHandler<CreateBarCommand>, ICommandHandler<SpeakCommand>
    {
        private readonly IRepository _repository;

        public BarCommandHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(CreateBarCommand command)
        {
            var bar = Bar.Create(command.AggregateId);

            await _repository.AddAsync(bar).ConfigureAwait(false);
        }

        public async Task ExecuteAsync(SpeakCommand command)
        {
            var bar = await _repository.GetByIdAsync<Bar>(command.AggregateId).ConfigureAwait(false);

            bar.Speak(command.Text);
        }
    }
}