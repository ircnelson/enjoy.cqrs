using EnjoyCQRS.Events;

namespace EnjoyCQRS.Bus
{
    public interface IEventRouter
    {
        void Route<TEvent>(TEvent @event) where TEvent : IDomainEvent;
    }
}