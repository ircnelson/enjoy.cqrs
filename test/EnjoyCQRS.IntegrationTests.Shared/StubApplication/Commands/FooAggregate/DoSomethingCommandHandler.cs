using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.IntegrationTests.Shared.StubApplication.Domain.FooAggregate;

namespace EnjoyCQRS.IntegrationTests.Shared.StubApplication.Commands.FooAggregate
{
    public class DoSomethingCommandHandler : ICommandHandler<DoSomethingCommand>
    {
        private readonly IRepository _repository;

        public DoSomethingCommandHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(DoSomethingCommand command)
        {
            var foo = await _repository.GetByIdAsync<Foo>(command.AggregateId);
            foo.DoSomething();
        }
    }
}