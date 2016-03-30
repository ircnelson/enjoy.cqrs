using MyCQRS.Commands;
using MyCQRS.EventStore;
using MyCQRS.Restaurant.Domain;

namespace MyCQRS.Restaurant.Commands.Handlers
{
    public class OpenTabCommandHandler : ICommandHandler<OpenTabCommand>
    {
        private readonly IDomainRepository _domainRepository;

        public OpenTabCommandHandler(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public void Execute(OpenTabCommand command)
        {
            var tab = TabAggregate.Create(command.TableNumber, command.Waiter);

            _domainRepository.Add(tab);
        }
    }
}