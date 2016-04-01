using MyCQRS.Commands;
using MyCQRS.EventStore;
using MyCQRS.Restaurant.Commands;
using MyCQRS.Restaurant.Domain;

namespace MyCQRS.Restaurant.CommandsHandlers
{
    public class MarkFoodServedCommandHandler : ICommandHandler<MarkFoodServedCommand>
    {
        private readonly IDomainRepository _domainRepository;

        public MarkFoodServedCommandHandler(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public void Execute(MarkFoodServedCommand command)
        {
            var tab = _domainRepository.GetById<TabAggregate>(command.AggregateId);
            tab.MarkFoodServed(command.MenuNumbers);
        }
    }
}