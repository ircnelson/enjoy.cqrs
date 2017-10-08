using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate;
using System.Threading.Tasks;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.UserAggregate
{
    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
    {
        private readonly IRepository _repository;

        public CreateUserCommandHandler(IRepository repository)
        {
            _repository = repository;
        }

        public Task ExecuteAsync(CreateUserCommand command)
        {
            var user = new User(command.AggregateId, command.FirstName, command.LastName, command.BornDate);

            return _repository.AddAsync(user);
        }
    }
}
