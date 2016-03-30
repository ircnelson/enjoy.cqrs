using MyCQRS.Commands;
using MyCQRS.EventStore;
using MyCQRS.Restaurant.Domain;

namespace MyCQRS.Restaurant.Commands.Handlers
{
    public class MarkDrinksServedCommandHandler : ICommandHandler<MarkDrinksServedCommand>
    {
        private readonly IDomainRepository _domainRepository;

        public MarkDrinksServedCommandHandler(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public void Execute(MarkDrinksServedCommand command)
        {
            var tab = _domainRepository.GetById<TabAggregate>(command.AggregateId);
            tab.MarkDrinksServed(command.MenuNumbers);
        }
    }
}