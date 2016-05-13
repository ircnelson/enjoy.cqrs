using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;

namespace EnjoyCQRS.IntegrationTests.Stubs
{
    public class ChangeNameCommandHandler : ICommandHandler<ChangeNameCommand>
    {
        private readonly IRepository _repository;

        public ChangeNameCommandHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(ChangeNameCommand command)
        {
            var aggregate = await _repository.GetByIdAsync<FakePerson>(command.AggregateId);
            aggregate.ChangeName(command.Name);

            await _repository.AddAsync(aggregate);
        }
    }
}