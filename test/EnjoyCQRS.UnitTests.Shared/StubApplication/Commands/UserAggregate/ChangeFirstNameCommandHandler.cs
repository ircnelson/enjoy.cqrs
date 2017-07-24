using EnjoyCQRS.Commands;
using EnjoyCQRS.EventSource.Storage;
using EnjoyCQRS.UnitTests.Shared.StubApplication.Domain.UserAggregate;
using System.Threading.Tasks;

namespace EnjoyCQRS.UnitTests.Shared.StubApplication.Commands.UserAggregate
{
    public class ChangeFirstNameCommandHandler : ICommandHandler<ChangeFirstNameCommand>
    {
        private readonly IRepository _repository;

        public ChangeFirstNameCommandHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(ChangeFirstNameCommand command)
        {
            var user = await _repository.GetByIdAsync<User>(command.AggregateId);

            user.ChangeFirstName(command.Name);
        }
    }
}
