using MyCQRS.Events;
using MyCQRS.Restaurant.Read;
using MyCQRS.Restaurant.Read.Models;

namespace MyCQRS.Restaurant.Events.Handlers
{
    public class TabOpenedEventHandler : IEventHandler<TabOpenedEvent>
    {
        private readonly IReadRepository<TabModel> _tabRepository;

        public TabOpenedEventHandler(IReadRepository<TabModel> tabRepository)
        {
            _tabRepository = tabRepository;
        }

        public void Execute(TabOpenedEvent theEvent)
        {
            var tab = new TabModel(theEvent.AggregateId, theEvent.TableNumber, theEvent.Waiter);
            _tabRepository.Insert(tab);
        }
    }
}