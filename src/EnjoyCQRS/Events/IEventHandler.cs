using System.Threading.Tasks;

namespace EnjoyCQRS.Events
{
    public interface IEventHandler<TEvent> where TEvent : IDomainEvent
    {
        Task ExecuteAsync(TEvent @event);
    }
}