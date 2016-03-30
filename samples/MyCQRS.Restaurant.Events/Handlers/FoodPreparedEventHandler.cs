using System.Linq;
using MyCQRS.Events;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;

namespace MyCQRS.Restaurant.Events.Handlers
{
    public class FoodPreparedEventHandler : IEventHandler<FoodPreparedEvent>
    {
        private readonly IReadRepository<TabModel> _tabRepository;

        public FoodPreparedEventHandler(IReadRepository<TabModel> tabRepository)
        {
            _tabRepository = tabRepository;
        }


        public void Execute(FoodPreparedEvent theEvent)
        {
            var tab = _tabRepository.GetById(theEvent.AggregateId);

            foreach (var menuNumber in theEvent.MenuNumbers)
            {
                var orderedItem = tab.OrderedItems.FirstOrDefault(e => e.MenuNumber == menuNumber);

                if (orderedItem != null)
                {
                    orderedItem.Status = "Prepared";

                    _tabRepository.Update(tab);
                }
            }
        }
    }
}