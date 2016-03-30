using System;
using MyCQRS.Events;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;

namespace MyCQRS.Restaurant.Events.Handlers
{
    public class FoodOrderedEventHandler : IEventHandler<FoodOrderedEvent>
    {
        private readonly IReadRepository<OrderItemModel> _repository;

        public FoodOrderedEventHandler(IReadRepository<OrderItemModel> repository)
        {
            _repository = repository;
        }

        public void Execute(FoodOrderedEvent theEvent)
        {
            var orderItemModel = new OrderItemModel(Guid.NewGuid(), theEvent.AggregateId, theEvent.MenuNumber, theEvent.Price, theEvent.Status);

            _repository.Insert(orderItemModel);
        }
    }
}