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

        public void Execute(ChangeNameCommand command)
        {
            var aggregate = _repository.GetById<FakePerson>(command.AggregateId);
            aggregate.ChangeName(command.Name);

            _repository.Add(aggregate);
        }
    }
}