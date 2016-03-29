using MyCQRS.Commands;
using MyCQRS.Domain;
using MyCQRS.Domain.StubBoundedContext;

namespace MyCQRS.CommandHandlers
{
    public class ChangeNameCommandHandler : ICommandHandler<ChangeNameCommand>
    {
        private readonly IDomainRepository _domainRepository;

        public ChangeNameCommandHandler(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public void Execute(ChangeNameCommand command)
        {
            var subject = _domainRepository.GetById<AggregateStub>(command.AggregateId);
            subject.ChangeName(command.Name);

            _domainRepository.Add(subject);
        }
    }
}