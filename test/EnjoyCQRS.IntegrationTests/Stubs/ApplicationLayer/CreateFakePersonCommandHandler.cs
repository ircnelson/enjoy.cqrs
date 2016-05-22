using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Stubs.DomainLayer;

namespace EnjoyCQRS.IntegrationTests.Stubs.ApplicationLayer
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

            _repository.AddAsync(fakePerson);

            return Task.CompletedTask;
        }
    }
}