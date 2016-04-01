using MyCQRS.Commands;
using MyCQRS.EventStore;
using MyCQRS.Restaurant.Commands;
using MyCQRS.Restaurant.Domain;

namespace MyCQRS.Restaurant.CommandsHandlers
{
    public class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
    {
        private readonly IDomainRepository _domainRepository;

        public PlaceOrderCommandHandler(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public void Execute(PlaceOrderCommand command)
        {
            var tab = _domainRepository.GetById<TabAggregate>(command.AggregateId);
            tab.PlaceOrder(command.OrderedItems);
        }
    }
}