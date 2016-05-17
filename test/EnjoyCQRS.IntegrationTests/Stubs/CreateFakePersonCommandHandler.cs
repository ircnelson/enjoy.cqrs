using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class CreateFakePersonCommandHandler : ICommandHandler<CreateFakePersonCommand>
    {
        private readonly IRepository _repository;

        public CreateFakePersonCommandHandler(IRepository repository)
        {
            _repository = repository;
        }

        public Task ExecuteAsync(CreateFakePersonCommand command)
        {
            var fakePerson = new FakePerson(command.AggregateId, command.Name);

            _repository.Add(fakePerson);

            return Task.CompletedTask;
        }
    }
}