using System.Threading.Tasks;
using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.Owin.IntegrationTests.Application.Domain.FooAggregate;

namespace EnjoyCQRS.Owin.IntegrationTests.Application.Commands
{
    public class CreateFooCommandHandler : ICommandHandler<CreateFooCommand>
    {
        private readonly IRepository _repository;

        public CreateFooCommandHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(CreateFooCommand command)
        {
            var foo = new Foo(command.AggregateId);
            await _repository.AddAsync(foo);
        }
    }
}