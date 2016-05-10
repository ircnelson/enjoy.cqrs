using EnjoyCQRS.Events;

namespace EnjoyCQRS.Bus
{
    public interface IEventRouter
    {
        void Route(IDomainEvent @event);
    }
}