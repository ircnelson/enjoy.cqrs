using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FakeGameAggregate;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FakeGameAggregate
{
    public class CreateFakeGameCommandHandler : ICommandHandler<CreateFakeGameCommand>
    {
        private readonly IRepository _repository;

        public CreateFakeGameCommandHandler(IRepository repository)
        {
            _repository = repository;
        }
        
        public Task ExecuteAsync(CreateFakeGameCommand command)
        {
            var fakeGame = new FakeGame(command.AggregateId, command.PlayerOneName, command.PlayerTwoName);

            _repository.AddAsync(fakeGame);

            return Task.CompletedTask;
        }
    }
}