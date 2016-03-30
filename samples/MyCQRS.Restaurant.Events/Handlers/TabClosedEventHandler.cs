using MyCQRS.Events;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;

namespace MyCQRS.Restaurant.Events.Handlers
{
    public class TabClosedEventHandler : IEventHandler<TabClosedEvent>
    {
        private readonly IReadRepository<TabModel> _tabRepository;

        public TabClosedEventHandler(IReadRepository<TabModel> tabRepository)
        {
            _tabRepository = tabRepository;
        }

        public void Execute(TabClosedEvent theEvent)
        {
            var tabModel = _tabRepository.GetById(theEvent.AggregateId);

            if (tabModel != null)
            {
                tabModel.AmountPaid = theEvent.AmountPaid;
                tabModel.IsOpen = false;
                tabModel.OrderedValue = theEvent.OrdersValue;
                tabModel.TipValue = theEvent.TipValue;
            }

            _tabRepository.Update(tabModel);
        }
    }
}