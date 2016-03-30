using MyCQRS.Commands;
using MyCQRS.EventStore;
using MyCQRS.Restaurant.Domain;

namespace MyCQRS.Restaurant.Commands.Handlers
{
    public class MarkFoodPreparedCommandHandler : ICommandHandler<MarkFoodPreparedCommand>
    {
        private readonly IDomainRepository _domainRepository;

        public MarkFoodPreparedCommandHandler(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public void Execute(MarkFoodPreparedCommand command)
        {
            var tab = _domainRepository.GetById<TabAggregate>(command.AggregateId);
            tab.MarkFoodPrepared(command.MenuNumbers);
        }
    }
}