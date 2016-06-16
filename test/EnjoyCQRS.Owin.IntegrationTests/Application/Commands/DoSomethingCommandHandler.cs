using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Owin.IntegrationTests.Application.Domain.FooAggregate;

namespace EnjoyCQRS.Owin.IntegrationTests.Application.Commands
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