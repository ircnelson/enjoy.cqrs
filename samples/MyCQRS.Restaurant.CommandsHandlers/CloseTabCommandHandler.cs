using EnjoyCQRS.Commands;
using EnjoyCQRS.EventStore;
using MyCQRS.Restaurant.Commands;
using MyCQRS.Restaurant.Domain;

namespace MyCQRS.Restaurant.CommandsHandlers
{
    public class CloseTabCommandHandler : ICommandHandler<CloseTabCommand>
    {
        private readonly IDomainRepository _domainRepository;

        public CloseTabCommandHandler(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }
        public void Execute(CloseTabCommand command)
        {
            var tab = _domainRepository.GetById<TabAggregate>(command.AggregateId);
            tab.CloseTab(command.AmountPaid);
        }
    }
}