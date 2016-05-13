using System.Collections.Generic;
using System.Threading.Tasks;
using EnjoyCQRS.Bus;
using EnjoyCQRS.Events;

namespace EnjoyCQRS.Messages
{
    public interface IEventPublisher : ITransactionalMessageBus
    {
        /// <summary>
        /// Publishes the event to the handler(s).
        /// </summary>
        Task PublishAsync<TEvent>(TEvent message) where TEvent : IDomainEvent;

        /// <summary>
        /// Publishes the event to the handler(s).
        /// </summary>
        Task PublishAsync<TEvent>(IEnumerable<TEvent> messages) where TEvent : IDomainEvent;
    }
}