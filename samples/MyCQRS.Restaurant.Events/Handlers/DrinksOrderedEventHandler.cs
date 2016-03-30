using System;
using MyCQRS.Events;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;

namespace MyCQRS.Restaurant.Events.Handlers
{
    public class DrinksOrderedEventHandler : IEventHandler<DrinksOrderedEvent>
    {
        private readonly IReadRepository<TabModel> _repository;

        public DrinksOrderedEventHandler(IReadRepository<TabModel> repository)
        {
            _repository = repository;
        }

        public void Execute(DrinksOrderedEvent theEvent)
        {
            var tab = _repository.GetById(theEvent.AggregateId);

            var newOrder = new OrderItemModel(Guid.NewGuid(), theEvent.AggregateId, theEvent.MenuNumber, theEvent.Price, theEvent.Status);
            tab.OrderedItems.Add(newOrder);

            _repository.Update(tab);
        }
    }
}