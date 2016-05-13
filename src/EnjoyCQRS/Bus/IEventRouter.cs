using System.Threading.Tasks;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Bus
{
    public interface IEventRouter
    {
        Task RouteAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;
    }
}