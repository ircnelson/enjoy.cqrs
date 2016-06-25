using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.BarAggregate;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.BarAggregate
{
    public class BarCommandHandlers : ICommandHandler<SpeakCommand>, ICommandHandler<CreateBarCommand>
    {
        private readonly IRepository _repository;

        public BarCommandHandlers(IRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(SpeakCommand command)
        {
            var bar = await _repository.GetByIdAsync<Bar>(command.AggregateId);

            bar.Speak(command.Text);
        }

        public async Task ExecuteAsync(CreateBarCommand command)
        {
            var bar = Bar.Create(command.AggregateId);

            await _repository.AddAsync(bar);
        }
    }
}